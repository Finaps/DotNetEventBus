using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Finaps.EventBus.Core;
using Finaps.EventBus.Core.Abstractions;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Finaps.EventBus.RabbitMQ
{
  public class RabbitMQEventSubscriber : IEventSubscriber
  {
    private readonly IRabbitMQPersistentConnection _persistentConnection;
    private readonly string _exchangeName;
    private readonly string _queueName;
    private readonly ILogger<RabbitMQEventSubscriber> _logger;
    private readonly int _retryCount;
    private IModel _consumerChannel;
    private bool _disposed;

    public RabbitMQEventSubscriber(
        IRabbitMQPersistentConnection persistentConnection,
        string exchangeName,
        string queueName,
        ILogger<RabbitMQEventSubscriber> logger,
        int retryCount
    )
    {
      _persistentConnection = persistentConnection;
      _exchangeName = exchangeName;
      _queueName = queueName;
      _logger = logger;
      _retryCount = retryCount;

      _consumerChannel = CreateConsumerChannel();
    }
    public event Core.Abstractions.AsyncEventHandler<IntegrationEventReceivedArgs> OnEventReceived;

    public void Subscribe(string eventName)
    {
      DoInternalSubscription(eventName);
      StartBasicConsume();
    }

    private void DoInternalSubscription(string eventName)
    {
      if (!_persistentConnection.IsConnected)
      {
        _persistentConnection.TryConnect();
      }

      using (var channel = _persistentConnection.CreateModel())
      {
        channel.QueueBind(queue: _queueName,
                          exchange: _exchangeName,
                          routingKey: eventName);
      }
    }

    public void Dispose()
    {
      if (_disposed) return;

      _disposed = true;

      try
      {
        if (_consumerChannel != null)
        {
          _consumerChannel.Dispose();
        }
        _persistentConnection.Dispose();
      }
      catch (IOException ex)
      {
        _logger.LogCritical(ex.ToString());
      }
    }

    private void StartBasicConsume()
    {
      _logger.LogTrace("Starting RabbitMQ basic consume");

      if (_consumerChannel != null)
      {
        var consumer = new AsyncEventingBasicConsumer(_consumerChannel);

        consumer.Received += Consumer_Received;

        _consumerChannel.BasicConsume(
            queue: _queueName,
            autoAck: false,
            consumer: consumer);
      }
      else
      {
        _logger.LogError("StartBasicConsume can't call on _consumerChannel == null");
      }
    }

    private async Task Consumer_Received(object sender, BasicDeliverEventArgs eventArgs)
    {
      var eventName = eventArgs.RoutingKey;
      var message = Encoding.UTF8.GetString(eventArgs.Body);

      var integrationEventReceivedArgs = new IntegrationEventReceivedArgs()
      {
        EventName = eventName,
        Message = message
      };
      try
      {
        await OnEventReceived?.Invoke(this, integrationEventReceivedArgs);
      }

      catch (Exception ex)
      {
        _logger.LogWarning(ex, "----- ERROR Processing message \"{Message}\"", message);
      }

      // Even on exception we take the message off the queue.
      // in a REAL WORLD app this should be handled with a Dead Letter Exchange (DLX). 
      // For more information see: https://www.rabbitmq.com/dlx.html
      _consumerChannel.BasicAck(eventArgs.DeliveryTag, multiple: false);
    }

    private IModel CreateConsumerChannel()
    {
      if (!_persistentConnection.IsConnected)
      {
        _persistentConnection.TryConnect();
      }

      _logger.LogTrace("Creating RabbitMQ consumer channel");

      var channel = _persistentConnection.CreateModel();

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
        _logger.LogWarning(ea.Exception, "Recreating RabbitMQ consumer channel");

        _consumerChannel.Dispose();
        _consumerChannel = CreateConsumerChannel();
        StartBasicConsume();
      };

      return channel;
    }
  }
}