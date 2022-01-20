using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Finaps.EventBus.Core.Abstractions;
using Finaps.EventBus.RabbitMq.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Client;

namespace Finaps.EventBus.RabbitMq
{
  internal class RabbitMqEventPublisher : IEventPublisher
  {
    private readonly IRabbitMqPersistentConnection _persistentConnection;
    private readonly string _exchangeName;
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private ObjectPool<IModel> _channelPool;
    private bool _disposed;

    internal RabbitMqEventPublisher(
        IRabbitMqPersistentConnection persistentConnection,
        string exchangeName,
        ILogger<RabbitMqEventPublisher> logger
    )
    {
      _persistentConnection = persistentConnection;
      _exchangeName = exchangeName;
      _logger = logger;
      _channelPool = CreateChannelPool();

      _persistentConnection.ConnectionLost += OnConnectionLost;
    }

    private void OnConnectionLost(object sender, EventArgs e)
    {
      _channelPool = CreateChannelPool();
    }

    private void Publish(string message, string eventName, string messageId)
    {
      _logger.LogTrace("Publishing event {MessageId} to RabbitMQ: ({EventName})", messageId, eventName);
      _logger.LogTrace("Publishing event {MessageId} to RabbitMQ. Retrieving channel..", messageId);
      var channel = _channelPool.Get();

      PublishMessage(channel, message, eventName, messageId);
      _channelPool.Return(channel);

    }

    public Task PublishAsync(string message, string eventName, string messageId)
    {
      Publish(message, eventName, messageId);
      return Task.CompletedTask;
    }

    private void PublishMessage(IModel channel, string message, string eventName, string messageId)
    {
      var body = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(message));
      var properties = channel.CreateBasicProperties();
      properties.DeliveryMode = 2; // persistent
      properties.MessageId = messageId;

      _logger.LogTrace("Publishing event to RabbitMQ: {Message} ({EventName})", message, eventName);
      channel.BasicPublish(
                    exchange: _exchangeName,
                    routingKey: eventName,
                    mandatory: true,
                    basicProperties: properties,
                    body: body);
    }

    private ObjectPool<IModel> CreateChannelPool()
    {
      var channelPoolPolicy = new ChannelPoolPolicy(_persistentConnection, _exchangeName);
      var channelPoolProvider = new DefaultObjectPoolProvider()
      {
        MaximumRetained = 10
      };
      return channelPoolProvider.Create(channelPoolPolicy);
    }


    private class ChannelPoolPolicy : PooledObjectPolicy<IModel>
    {

      private readonly IRabbitMqPersistentConnection _connection;
      private readonly string _exchangeName;

      public ChannelPoolPolicy(IRabbitMqPersistentConnection connection, string exchangeName)
      {
        this._connection = connection;
        this._exchangeName = exchangeName;
      }

      public override IModel Create()
      {
        var channel = _connection.CreateModel();
        channel.ExchangeDeclare(_exchangeName, "direct", true);
        return channel;
      }

      public override bool Return(IModel obj)
      {
        return obj != null && obj.IsOpen;
      }
    }

    public ValueTask DisposeAsync()
    {
      if (_disposed) return new ValueTask();

      _disposed = true;
      _logger.LogTrace($"Disposing {this.GetType().Name}");

      try
      {
        _persistentConnection.Dispose();
      }
      catch (IOException ex)
      {
        _logger.LogCritical(ex.ToString());
      }
      _logger.LogTrace($"{this.GetType().Name} disposed");
      return new ValueTask();
    }


  }
}