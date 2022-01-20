using System;
using System.Collections.Generic;
using Finaps.EventBus.Core;
using Finaps.EventBus.Core.Abstractions;
using Finaps.EventBus.Core.Models;
using Finaps.EventBus.RabbitMq.Abstractions;
using Finaps.EventBus.RabbitMq.Configuration;
using Finaps.EventBus.RabbitMq.Connection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RabbitMQ.Client;

namespace Finaps.EventBus.RabbitMq
{

  public static class RabbitMqEventBusFactory
  {
    public static IEventBus Create(IServiceProvider serviceProvider, List<EventSubscription> subscriptions, RabbitMqOptions options = null, ILoggerFactory loggerFactory = null)
    {
      loggerFactory ??= new NullLoggerFactory();
      options ??= new RabbitMqOptions();

      ConnectionFactory connectionFactory = CreateFactory(options);
      var subscriptionsManager = new InMemoryEventBusSubscriptionsManager();
      IRabbitMqPersistentConnection publisherConnection = CreateConnection(loggerFactory, connectionFactory, options.ConnectRetryCount, options.QueueName + "-publisher");
      IRabbitMqPersistentConnection subscriberConnection = CreateConnection(loggerFactory, connectionFactory, options.ConnectRetryCount, options.QueueName + "-subscriber");
      RabbitMqEventPublisher publisher = CreatePublisher(publisherConnection, options, loggerFactory);
      RabbitMqEventSubscriber subscriber = CreateSubscriber(subscriberConnection, options, loggerFactory);
      var logger = loggerFactory.CreateLogger<IEventBus>();
      var eventBus = new RabbitMqEventBus(publisher, subscriber, subscriptionsManager, serviceProvider, logger);
      foreach (var subscription in subscriptions)
      {
        eventBus.AddSubscription(subscription);
      }

      return eventBus;
    }

    private static RabbitMqEventPublisher CreatePublisher(IRabbitMqPersistentConnection connection,
      RabbitMqOptions options, ILoggerFactory loggerFactory)
    {
      var logger = loggerFactory.CreateLogger<RabbitMqEventPublisher>();
      return new RabbitMqEventPublisher(connection, options.ExchangeName, logger);
    }

    private static RabbitMqEventSubscriber CreateSubscriber(IRabbitMqPersistentConnection connection,
      RabbitMqOptions options, ILoggerFactory loggerFactory)
    {
      var logger = loggerFactory.CreateLogger<RabbitMqEventSubscriber>();
      return new RabbitMqEventSubscriber(connection, options.ExchangeName, options.QueueName, logger, options.PrefetchCount);
    }

    private static IRabbitMqPersistentConnection CreateConnection(ILoggerFactory loggerFactory,
      ConnectionFactory factory, int retryAttempts, string name)
    {
      var logger = loggerFactory.CreateLogger<DefaultRabbitMqPersistentConnection>();
      var connection =
        new DefaultRabbitMqPersistentConnection(factory, logger, retryAttempts, name);
      return connection;
    }

    private static ConnectionFactory CreateFactory(RabbitMqOptions options)
    {
      return new ConnectionFactory()
      {
        DispatchConsumersAsync = true,
        HostName = options.HostName,
        UserName = options.UserName,
        Password = options.Password,
        VirtualHost = options.VirtualHost,
        AutomaticRecoveryEnabled = false,
        TopologyRecoveryEnabled = false
      };
    }

  }
}