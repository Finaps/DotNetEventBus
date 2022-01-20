
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Finaps.EventBus.RabbitMq.Abstractions;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Finaps.EventBus.RabbitMq.Connection
{
  internal class DefaultRabbitMqPersistentConnection
       : IRabbitMqPersistentConnection
  {
    private readonly Policy _initialConnectPolicy;
    private readonly IConnectionFactory _connectionFactory;
    private readonly ILogger _logger;
    private readonly string _name;
    private IConnection _connection;
    private bool _disposed;

    object _syncRoot = new object();

    internal DefaultRabbitMqPersistentConnection(IConnectionFactory connectionFactory, ILogger<DefaultRabbitMqPersistentConnection> logger, int retryCount = 5, string name = "connection")
    {
      _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _name = name;
      _initialConnectPolicy = CreateInitialConnectionPolicy(retryCount);
      CreateInitialConnection();
    }

    public event EventHandler<EventArgs> ConnectionRecovered;
    public event EventHandler<EventArgs> ConnectionLost;

    public bool IsConnected => _connection != null && _connection.IsOpen && !_disposed;

    public IModel CreateModel()
    {
      _logger.LogTrace("Creating channel on connection {Name}", _name);
      if (!WaitForConnection(100).Wait(TimeSpan.FromSeconds(60)))
      {
        throw new TimeoutException($"Timed out while waiting for connection on {_name}");
      }
      return _connection.CreateModel();
    }

    private async Task WaitForConnection(int delay)
    {
      while (!IsConnected) await Task.Delay(delay);
    }

    private void CreateInitialConnection()
    {
      _logger.LogInformation("RabbitMQ Client is trying to create connection {Name}", _name);
      if (IsConnected) return;

      _initialConnectPolicy.Execute(() =>
      {
        _connection = _connectionFactory
          .CreateConnection(_name);
      });

      AddFailureEventHandlers(_connection);

      _logger.LogInformation("RabbitMQ Client acquired a persistent connection {Name} to '{HostName}' and is subscribed to failure events", _name, _connection.Endpoint.HostName);

    }

    private void Reconnect()
    {
      _logger.LogInformation("RabbitMQ Client is trying to re-create connection {Name}", _name);
      lock (_syncRoot)
      {
        if (IsConnected) return;

        RemoveFailureEventHandlers(_connection);

        var policy = CreateRetryConnectionPolicy();

        try
        {
          policy.Execute(() =>
          {
            _connection = _connectionFactory
              .CreateConnection(_name);
          });
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Connection {Name} to RabbitMq could not be established", _name);
        }

        if (IsConnected)
        {
          AddFailureEventHandlers(_connection);

          ConnectionRecovered?.Invoke(this, EventArgs.Empty);

          _logger.LogInformation(
            "RabbitMQ Client acquired a persistent connection {Name} to '{HostName}' and is subscribed to failure events",
            _name, _connection.Endpoint.HostName);
        }
        else
        {
          _logger.LogCritical("FATAL ERROR: RabbitMQ connection {Name} could not be created and opened", _name);
        }
      }
    }

    private void AddFailureEventHandlers(IConnection connection)
    {
      connection.ConnectionShutdown += OnConnectionShutdown;
      connection.CallbackException += OnCallbackException;
      connection.ConnectionBlocked += OnConnectionBlocked;
    }

    private void RemoveFailureEventHandlers(IConnection connection)
    {
      connection.ConnectionShutdown -= OnConnectionShutdown;
      connection.CallbackException -= OnCallbackException;
      connection.ConnectionBlocked -= OnConnectionBlocked;
    }

    private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
    {
      if (_disposed) return;

      _logger.LogWarning("RabbitMQ connection {Name} is shutdown", _name);
      ConnectionLost?.Invoke(this, EventArgs.Empty);
      Reconnect();
    }

    private void OnCallbackException(object sender, CallbackExceptionEventArgs e)
    {
      if (_disposed) return;

      _logger.LogWarning("RabbitMQ connection {Name} throw exception {Message}", _name, e.Exception.Message);
      ConnectionLost?.Invoke(this, EventArgs.Empty);
      Reconnect();
    }

    private void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
    {
      if (_disposed) return;

      _logger.LogWarning("RabbitMQ connection {Name} is on shutdown", _name);
      ConnectionLost?.Invoke(this, EventArgs.Empty);
      Reconnect();
    }

    private RetryPolicy CreateInitialConnectionPolicy(int retryCount)
    {
      var policy = Policy.Handle<SocketException>()
        .Or<BrokerUnreachableException>()
        .WaitAndRetry(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
          (ex, time) =>
          {
            _logger.LogWarning(ex,
              "RabbitMQ Client could not create connection {Name} after {TimeOut}s ({ExceptionMessage})", _name,
              $"{time.TotalSeconds:n1}", ex.Message);
          }
        );
      return policy;
    }

    private RetryPolicy CreateRetryConnectionPolicy()
    {
      var policy = Policy.Handle<SocketException>()
        .Or<BrokerUnreachableException>()
        .WaitAndRetryForever(_ => TimeSpan.FromSeconds(5),
          (ex, time) =>
          {
            _logger.LogWarning("RabbitMQ Client could not re-create connection {Name} after {TimeOut}s ({ExceptionMessage})", _name,
              $"{time.TotalSeconds:n1}", ex.Message);
          }
        );
      return policy;
    }

    public void Dispose()
    {
      if (_disposed) return;

      _logger.LogTrace($"Disposing {this.GetType().Name}");

      _disposed = true;

      try
      {
        _connection?.Dispose();
      }
      catch (IOException ex)
      {
        _logger.LogCritical(ex.ToString());
      }
      _logger.LogTrace($"{this.GetType().Name} disposed");
    }
  }
}