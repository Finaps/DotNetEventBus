using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Finaps.EventBus.Core;
using Finaps.EventBus.Core.Abstractions;
using Finaps.EventBus.RabbitMq.Abstractions;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Finaps.EventBus.RabbitMq
{

  internal class RabbitMqEventSubscriber : IEventSubscriber
  {
    private readonly ushort _prefetchCount;
    private readonly Dictionary<string, bool> _handledUnAckedMessages;
    private readonly IRabbitMqPersistentConnection _persistentConnection;
    private readonly string _exchangeName;
    private readonly string _queueName;
    private readonly ILogger<RabbitMqEventSubscriber> _logger;
    private IModel _consumerChannel;
    private bool _disposed;
    private bool _startedConsuming;
    private bool _consuming;

    internal RabbitMqEventSubscriber(
        IRabbitMqPersistentConnection persistentConnection,
        string exchangeName,
        string queueName,
        ILogger<RabbitMqEventSubscriber> logger,
        ushort prefetchCount = 500
    )
    {
      _persistentConnection = persistentConnection;
      _exchangeName = exchangeName;
      _queueName = queueName;
      _logger = logger;
      _prefetchCount = prefetchCount;
      _handledUnAckedMessages = new Dictionary<string, bool>();

      _consumerChannel = CreateConsumerChannel();

      _persistentConnection.ConnectionRecovered += OnConnectionRecovered;
      _persistentConnection.ConnectionLost += OnConnectionLost;
    }

    public Task InitializeAsync()
    {
      return Task.CompletedTask;
    }

    public event Core.Utilities.AsyncEventHandler<IntegrationEventReceivedArgs> OnEventReceived;

    public Task SubscribeAsync(string eventName)
    {
      DoInternalSubscription(eventName);
      return Task.CompletedTask;
    }

    public Task StartConsumingAsync()
    {
      if (_startedConsuming) return Task.CompletedTask;
      StartBasicConsume();
      _startedConsuming = true;
      return Task.CompletedTask;
    }

    private void DoInternalSubscription(string eventName)
    {
      using var channel = _persistentConnection.CreateModel();
      _logger.LogDebug($"Create binding with routing key {eventName} on queue");
      channel.QueueBind(queue: _queueName,
        exchange: _exchangeName,
        routingKey: eventName);
    }

    private void StartBasicConsume()
    {
      if (_consuming) return;
      _logger.LogTrace("Starting RabbitMQ basic consume");

      if (_consumerChannel == null)
      {
        _logger.LogError("StartBasicConsume can't call on _consumerChannel == null");
        return;
      }

      var consumer = new AsyncEventingBasicConsumer(_consumerChannel);

      consumer.Received += OnMessageReceived;

      _consumerChannel.BasicConsume(
        queue: _queueName,
        autoAck: false,
        consumer: consumer);

      _consuming = true;
    }

    private void OnConnectionLost(object sender, EventArgs e)
    {
      _consuming = false;
      _consumerChannel.Close();
      _consumerChannel.Dispose();
    }

    private void OnConnectionRecovered(object sender, EventArgs args)
    {
      _consumerChannel = CreateConsumerChannel();
      StartBasicConsume();
    }

    private async Task OnMessageReceived(object sender, BasicDeliverEventArgs eventArgs)
    {
      var messageId = eventArgs.BasicProperties.MessageId;
      _logger.LogTrace($"Received message {messageId} with tag {eventArgs.DeliveryTag}");
      if (!string.IsNullOrEmpty(messageId) && _handledUnAckedMessages.TryGetValue(messageId, out var success))
      {
        _logger.LogDebug($"Message {messageId} was already handled but could not be acked. Acking it now and skipping further handling");
        if (success)
        {
          _consumerChannel.BasicAck(eventArgs.DeliveryTag, false);
        }
        else
        {
          _consumerChannel.BasicReject(eventArgs.DeliveryTag, false);
        }
        _handledUnAckedMessages.Remove(messageId);
        return;
      }
      var eventName = eventArgs.RoutingKey;
      var message = Encoding.UTF8.GetString(eventArgs.Body.Span);

      var integrationEventReceivedArgs = new IntegrationEventReceivedArgs()
      {
        EventName = eventName,
        Message = message
      };
      try
      {
        if (OnEventReceived != null) await OnEventReceived.Invoke(this, integrationEventReceivedArgs);
        AcknowledgeMessage(messageId, eventArgs.DeliveryTag);
      }

      catch (Exception ex)
      {
        _logger.LogWarning(ex, "----- ERROR Processing message \"{Message}\": {Exception}", message, ex.Message);
        RejectMessage(messageId, eventArgs.DeliveryTag);
      }
    }

    private void RejectMessage(string messageId, ulong deliveryTag)
    {
      if (_persistentConnection.IsConnected)
      {
        _consumerChannel.BasicReject(deliveryTag, false);
      }
      else
      {
        _logger.LogTrace($"Rabbitmq connection is down. Storing message id {messageId} so we will not handle it again");
        _handledUnAckedMessages[messageId] = false;
      }
    }

    private void AcknowledgeMessage(string messageId, ulong deliveryTag)
    {
      if (_persistentConnection.IsConnected)
      {
        _consumerChannel.BasicAck(deliveryTag, false);
      }
      else
      {
        _logger.LogTrace($"Rabbitmq connection is down. Storing message id {messageId} so we will not handle it again");
        _handledUnAckedMessages[messageId] = true;
      }
    }

    private IModel CreateConsumerChannel()
    {
      _logger.LogTrace("Creating RabbitMQ consumer channel");

      var channel = _persistentConnection.CreateModel();

      channel.BasicQos(0, _prefetchCount, false);

      channel.ExchangeDeclare(_exchangeName,
                              "direct",
                              durable: true);

      channel.QueueDeclare(queue: _queueName,
                           durable: true,
                           exclusive: false,
                           autoDelete: false,
                           arguments: null);

      channel.CallbackException += (sender, ea) =>
      {
        _logger.LogWarning(ea.Exception, "Error when trying to acknowledge message handling");
      };

      channel.ModelShutdown += (sender, args) =>
        _logger.LogDebug("Subscriber channel shutting down. Cause {Cause}", args.ReplyText);

      return channel;
    }

    public ValueTask DisposeAsync()
    {
      if (_disposed) return default;

      _disposed = true;
      _logger.LogTrace($"Disposing {this.GetType().Name}");
      try
      {
        _consumerChannel?.Dispose();
        _persistentConnection.Dispose();
      }
      catch (IOException ex)
      {
        _logger.LogCritical(ex.ToString());
      }
      _logger.LogTrace($"{this.GetType().Name} disposed");

      return default;
    }
  }
}