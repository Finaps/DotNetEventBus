using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Finaps.EventBus.Core.Abstractions;
using Finaps.EventBus.Core.Events;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Finaps.EventBus.RabbitMQ
{
  public class RabbitMQEventPublisher : IEventPublisher
  {
    private readonly IRabbitMQPersistentConnection _persistentConnection;
    private readonly string _exchangeName;
    private readonly ILogger<RabbitMQEventPublisher> _logger;
    private readonly int _retryCount;
    private bool _disposed;

    public RabbitMQEventPublisher(
        IRabbitMQPersistentConnection persistentConnection,
        string exchangeName,
        ILogger<RabbitMQEventPublisher> logger,
        int retryCount
    )
    {
      _persistentConnection = persistentConnection;
      _exchangeName = exchangeName;
      _logger = logger;
      _retryCount = retryCount;
    }

    public void Dispose()
    {
      if (_disposed) return;

      _disposed = true;

      try
      {
        _persistentConnection.Dispose();
      }
      catch (IOException ex)
      {
        _logger.LogCritical(ex.ToString());
      }
    }

    public void Publish(IntegrationEvent @event)
    {
      if (!_persistentConnection.IsConnected)
      {
        _persistentConnection.TryConnect();
      }

      var policy = RetryPolicy.Handle<BrokerUnreachableException>()
          .Or<SocketException>()
          .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
          {
            _logger.LogWarning(ex, "Could not publish event: {EventId} after {Timeout}s ({ExceptionMessage})", @event.Id, $"{time.TotalSeconds:n1}", ex.Message);
          });

      var eventName = @event.GetType().Name;

      _logger.LogTrace("Creating RabbitMQ channel to publish event: {EventId} ({EventName})", @event.Id, eventName);

      using (var channel = _persistentConnection.CreateModel())
      {

        _logger.LogTrace("Declaring RabbitMQ exchange to publish event: {EventId}", @event.Id);

        channel.ExchangeDeclare(_exchangeName, "direct", true);

        var message = JsonConvert.SerializeObject(@event);
        var body = Encoding.UTF8.GetBytes(message);

        policy.Execute(() =>
        {
          var properties = channel.CreateBasicProperties();
          properties.DeliveryMode = 2; // persistent

          _logger.LogTrace("Publishing event to RabbitMQ: {EventId}", @event.Id);

          channel.BasicPublish(
                      exchange: _exchangeName,
                      routingKey: eventName,
                      mandatory: true,
                      basicProperties: properties,
                      body: body);
        });
      }
    }


  }
}