using System;
using System.Collections.Generic;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Finaps.EventBus.AzureServiceBus.Configuration;
using Finaps.EventBus.Core;
using Finaps.EventBus.Core.Abstractions;
using Finaps.EventBus.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Finaps.EventBus.AzureServiceBus
{
  public static class AzureServiceBusEventBusFactory
  {
    public static IEventBus Create(IServiceProvider serviceProvider, List<EventSubscription> subscriptions, AzureServiceBusOptions options = null, ILoggerFactory loggerFactory = null)
    {
      loggerFactory ??= new NullLoggerFactory();
      options ??= new AzureServiceBusOptions();

      var subscriptionsManager = new InMemoryEventBusSubscriptionsManager();
      var publisherClient = CreateClient(options);
      var subscriberClient = CreateClient(options);
      var adminClient = CreateAdminClient(options);
      var publisher = CreatePublisher(publisherClient, options, loggerFactory);
      var subscriber = CreateSubscriber(subscriberClient, adminClient, options, loggerFactory);
      var logger = loggerFactory.CreateLogger<IEventBus>();
      var eventBus = new AzureServiceBusEventBus(publisher, subscriber, subscriptionsManager, serviceProvider, logger);
      foreach (var subscription in subscriptions)
      {
        eventBus.AddSubscription(subscription);
      }

      return eventBus;
    }

    private static ServiceBusAdministrationClient CreateAdminClient(AzureServiceBusOptions options)
    {
      return new ServiceBusAdministrationClient(options.ConnectionString);
    }

    private static ServiceBusClient CreateClient(AzureServiceBusOptions options)
    {
      return new ServiceBusClient(options.ConnectionString);
    }

    private static AzureServiceBusEventPublisher CreatePublisher(ServiceBusClient client, AzureServiceBusOptions options, ILoggerFactory loggerFactory)
    {
      var logger = loggerFactory.CreateLogger<AzureServiceBusEventPublisher>();
      return new AzureServiceBusEventPublisher(client, options, logger);
    }

    private static AzureServiceBusEventSubscriber CreateSubscriber(ServiceBusClient client,
      ServiceBusAdministrationClient adminClient, AzureServiceBusOptions options, ILoggerFactory loggerFactory)
    {
      var logger = loggerFactory.CreateLogger<AzureServiceBusEventSubscriber>();
      return new AzureServiceBusEventSubscriber(client, adminClient, options, logger);
    }
  }
}