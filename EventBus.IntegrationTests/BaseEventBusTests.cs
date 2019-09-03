using System;
using System.Threading;
using Finaps.EventBus.AzureServiceBus;
using Finaps.EventBus.AzureServiceBus.DependencyInjection;
using Finaps.EventBus.Core.Abstractions;
using Finaps.EventBus.InMemory.DependencyInjection;
using Finaps.EventBus.IntegrationTests.Events;
using Finaps.EventBus.RabbitMQ;
using Finaps.EventBus.RabbitMQ.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Finaps.EventBus.IntegrationTests
{
  public abstract class BaseEventBusTests : IDisposable
  {
    protected EventReceivedNotifier eventReceivedNotifier;
    protected AutoResetEvent autoResetEvent;
    protected IEventBus eventBus;
    protected BaseEventBusTests(EventBusType eventBusType)
    {
      eventReceivedNotifier = new EventReceivedNotifier();
      autoResetEvent = new AutoResetEvent(false);
      eventReceivedNotifier.OnEventReceived += (s, e) =>
      {
        autoResetEvent.Set();
      };
      var services = SetupServices(eventReceivedNotifier);
      eventBus = SetupEventBus(services, eventBusType);
    }

    private static ServiceCollection SetupServices(EventReceivedNotifier eventReceivedNotifier)
    {
      var services = new ServiceCollection();
      services.AddSingleton<EventReceivedNotifier>(eventReceivedNotifier);
      services.AddScoped<EventPublisherEventHandler>();
      services.AddScoped<SubscriptionTestEventHandler>();
      services.AddSingleton<ILoggerFactory>(new NullLoggerFactory());
      services.AddLogging();
      return services;
    }

    private static IEventBus SetupEventBus(ServiceCollection services, EventBusType eventBusType)
    {

      switch (eventBusType)
      {
        case EventBusType.RabbitMQ:
          services.AddRabbitMQ(new RabbitMQOptions()
          {
            ExchangeName = "Exchange",
            QueueName = "IntegrationTests",
            UserName = "guest",
            Password = "guest"
          });
          break;
        case EventBusType.In_Memory:
          services.AddInMemoryEventBus();
          break;
        case EventBusType.AZURE:
          services.AddAzureServiceBus(new AzureServiceBusOptions()
          {
            SubscriptionName = "IntegrationTest",
            ConnectionString = "Endpoint=sb://finaps-bus.servicebus.windows.net/;SharedAccessKeyName=IntegrationTest;SharedAccessKey=x55a0K04JdGS+5/uFNTw99raxFFUY5T7iq2UFBbZbBg=;EntityPath=test"
          });
          break;

      }


      var serviceProvider = new DefaultServiceProviderFactory().CreateServiceProvider(services);
      var eventBus = serviceProvider.GetRequiredService<IEventBus>();
      eventBus.Subscribe<EventPublisherEvent, EventPublisherEventHandler>();
      eventBus.Subscribe<SubscriptionTestEvent, SubscriptionTestEventHandler>();
      return eventBus;
    }

    protected SubscriptionTestEvent PublishSubscriptionTestEvent()
    {
      var subscriptionTestEvent = new SubscriptionTestEvent()
      {
        TestString = "test"
      };
      eventBus.Publish(subscriptionTestEvent);
      return subscriptionTestEvent;
    }
    public void Dispose()
    {
      autoResetEvent.Dispose();
      eventBus.Dispose();
    }
  }
}