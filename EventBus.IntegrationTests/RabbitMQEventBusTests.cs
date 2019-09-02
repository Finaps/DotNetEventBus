using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Finaps.EventBus.Core.Abstractions;
using Finaps.EventBus.IntegrationTests.Events;
using Finaps.EventBus.RabbitMQ;
using Finaps.EventBus.RabbitMQ.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Finaps.EventBus.IntegrationTests
{
  public class RabbitMQEventBusTests : IDisposable
  {

    EventReceivedNotifier eventReceivedNotifier;
    AutoResetEvent autoResetEvent;
    IEventBus eventBus;

    public RabbitMQEventBusTests()
    {
      eventReceivedNotifier = new EventReceivedNotifier();
      autoResetEvent = new AutoResetEvent(false);
      eventReceivedNotifier.OnEventReceived += (s, e) =>
      {
        autoResetEvent.Set();
      };
      eventBus = SetupEventBus(eventReceivedNotifier);
    }

    [Fact]
    public void ListensCorrectly()
    {
      var subscriptionTestEvent = PublishSubscriptionTestEvent();
      var eventReceived = autoResetEvent.WaitOne(20000);
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
      var eventReceived = autoResetEvent.WaitOne(20000);
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
      var eventReceived = autoResetEvent.WaitOne(1000);
      Assert.True(eventReceived);
      var publishedGuids = publishedEvents.Select(@event => @event.Id);
      var consumedGuids = eventReceivedNotifier.Events.Select(@event => @event.Id);
      Assert.Equal(publishedGuids, consumedGuids);
    }

    private SubscriptionTestEvent PublishSubscriptionTestEvent()
    {
      var subscriptionTestEvent = new SubscriptionTestEvent()
      {
        TestString = "test"
      };
      eventBus.Publish(subscriptionTestEvent);
      return subscriptionTestEvent;
    }

    private static IEventBus SetupEventBus(EventReceivedNotifier eventReceivedNotifier)
    {
      var services = new ServiceCollection();
      services.AddSingleton<EventReceivedNotifier>(eventReceivedNotifier);
      services.AddScoped<EventPublisherEventHandler>();
      services.AddScoped<SubscriptionTestEventHandler>();
      services.AddRabbitMQ(new RabbitMQOptions()
      {
        ExchangeName = "Exchange",
        QueueName = "IntegrationTests",
        UserName = "guest",
        Password = "guest"
      });
      services.AddSingleton(new NullLoggerFactory());
      services.AddLogging();
      var serviceProvider = new DefaultServiceProviderFactory().CreateServiceProvider(services);
      var eventBus = serviceProvider.GetRequiredService<IEventBus>();
      eventBus.Subscribe<EventPublisherEvent, EventPublisherEventHandler>();
      eventBus.Subscribe<SubscriptionTestEvent, SubscriptionTestEventHandler>();
      return eventBus;
    }

    public void Dispose()
    {
      autoResetEvent.Dispose();
      eventBus.Dispose();
    }
  }
}
