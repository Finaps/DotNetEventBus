using System;
using System.Collections.Generic;
using Finaps.EventBus.Kafka.Configuration;
using Finaps.EventBus.Core;
using Finaps.EventBus.Core.Abstractions;
using Finaps.EventBus.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using EventBus.Kafka;

namespace Finaps.EventBus.Kafka
{
  public static class KafkaFactory
  {
    public static IEventBus Create(IServiceProvider serviceProvider, List<EventSubscription> subscriptions, KafkaOptions? options = null, ILoggerFactory? loggerFactory = null)
    {
      loggerFactory ??= new NullLoggerFactory();
      options ??= new KafkaOptions();

      var subscriptionsManager = new InMemoryEventBusSubscriptionsManager();
      var publisher = CreatePublisher(options, loggerFactory);
      var consumer = CreateSubscriber(options, loggerFactory);
      var logger = loggerFactory.CreateLogger<IEventBus>();
      var eventBus = new KafkaEventBus(publisher, consumer, subscriptionsManager, serviceProvider, logger);
      foreach (var subscription in subscriptions)
      {
        eventBus.AddSubscription(subscription);
      }

      return eventBus;
    }

    private static KafkaEventPublisher CreatePublisher(KafkaOptions options, ILoggerFactory loggerFactory)
    {
      var logger = loggerFactory.CreateLogger<KafkaEventPublisher>();
      return new KafkaEventPublisher(logger);
    }

    private static KafkaEventSubscriber CreateSubscriber(KafkaOptions options, ILoggerFactory loggerFactory)
    {
      var logger = loggerFactory.CreateLogger<KafkaEventSubscriber>();
      return new KafkaEventSubscriber(logger);
    }
  }
}