using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventBus.IntegrationTests;
using EventBus.IntegrationTests.Events;
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
using Xunit;

namespace Finaps.EventBus.IntegrationTests
{
  public abstract class BaseEventBusTests : IDisposable
  {
    protected EventReceivedNotifier eventReceivedNotifier;
    protected IntegerIncrementer integerIncrementer;
    protected AutoResetEvent autoResetEvent;
    protected IEventBus eventBus;
    protected BaseEventBusTests(EventBusType eventBusType)
    {
      eventReceivedNotifier = new EventReceivedNotifier();
      integerIncrementer = new IntegerIncrementer();
      autoResetEvent = new AutoResetEvent(false);
      eventReceivedNotifier.OnEventReceived += (s, e) =>
      {
        autoResetEvent.Set();
      };
      var services = SetupServices();
      eventBus = SetupEventBus(services, eventBusType);
    }

    private ServiceCollection SetupServices()
    {
      var services = new ServiceCollection();
      services.AddSingleton<EventReceivedNotifier>(eventReceivedNotifier);
      services.AddSingleton<IntegerIncrementer>(integerIncrementer);
      services.AddScoped<EventPublisherEventHandler>();
      services.AddScoped<SubscriptionTestEventHandler>();
      services.AddScoped<CheckConcurrencyEventHandler>();
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
        case EventBusType.Azure:
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
      eventBus.Subscribe<CheckConcurrencyEvent, CheckConcurrencyEventHandler>();
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

    [Fact]
    public void ListensCorrectly()
    {
      var subscriptionTestEvent = PublishSubscriptionTestEvent();
      var eventReceived = autoResetEvent.WaitOne(5000);
      Assert.True(eventReceived);
      var consumedEvent = eventReceivedNotifier.Events.Single() as SubscriptionTestEvent;
      Assert.Equal(subscriptionTestEvent.TestString, consumedEvent.TestString);
      Assert.Equal(subscriptionTestEvent.Id, consumedEvent.Id);
      Assert.Equal(subscriptionTestEvent.CreationDate, consumedEvent.CreationDate);

    }

    [Fact]
    public void CanPublishWhileReceiving()
    {
      var eventPublisherEvent = new EventPublisherEvent();
      eventBus.Publish(eventPublisherEvent);
      var eventReceived = autoResetEvent.WaitOne(5000);
      Assert.True(eventReceived);
      Assert.NotEmpty(eventReceivedNotifier.Events);
    }

    [Fact]
    public void EventsAreReceivedInOrder()
    {
      var publishedEvents = new List<SubscriptionTestEvent>();
      for (int i = 0; i < 50; i++)
      {
        publishedEvents.Add(PublishSubscriptionTestEvent());
      }
      var eventReceived = autoResetEvent.WaitOne(5000);
      Assert.True(eventReceived);
      var publishedGuids = publishedEvents.Select(@event => @event.Id);
      var consumedGuids = eventReceivedNotifier.Events.Select(@event => @event.Id);
      Assert.Equal(publishedGuids, consumedGuids);
    }

    [Fact]
    public async Task EventsAreHandledSequentially()
    {
      for (int i = 0; i < 10; i++)
      {
        eventBus.Publish(new CheckConcurrencyEvent());
      }
      await Task.Delay(1000);
      Assert.Equal(10, integerIncrementer.TimesIncremented);
    }
  }
}