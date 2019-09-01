using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Finaps.EventBus.IntegrationTests.Events;
using System.Linq;
using Finaps.EventBus.Core.Abstractions;
using Finaps.EventBus.InMemory.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace Finaps.EventBus.IntegrationTests
{
  public class InMemoryEventBusTests
  {
    [Fact]
    public void ListensCorrectly()
    {
      var eventReceivedNotifier = new EventReceivedNotifier();
      var are = new AutoResetEvent(false);
      eventReceivedNotifier.OnEventReceived += (s, e) =>
      {
        are.Set();
      };
      var services = new ServiceCollection();
      services.AddSingleton<EventReceivedNotifier>(eventReceivedNotifier);
      services.AddScoped<SubscriptionTestEventHandler>();
      services.AddSingleton<ILogger>(new NullLoggerFactory().CreateLogger("logger"));
      services.AddInMemoryEventBus();
      var serviceProvider = new DefaultServiceProviderFactory().CreateServiceProvider(services);
      var eventBus = serviceProvider.GetRequiredService<IEventBus>();
      eventBus.Subscribe<SubscriptionTestEvent, SubscriptionTestEventHandler>();
      string testString = "test";
      var subscriptionTestEvent = new SubscriptionTestEvent()
      {
        TestString = testString
      };
      eventBus.Publish(subscriptionTestEvent);
      var eventReceived = are.WaitOne(5000);
      Assert.True(eventReceived);
      var consumedEvent = eventReceivedNotifier.Events.Single() as SubscriptionTestEvent;
      Assert.Equal(testString, consumedEvent.TestString);
      Assert.Equal(subscriptionTestEvent.Id, consumedEvent.Id);
      Assert.Equal(subscriptionTestEvent.CreationDate, consumedEvent.CreationDate);

    }

    [Fact]
    public void PublishingAfterConsumingWorks()
    {
      var eventReceivedNotifier = new EventReceivedNotifier();
      var are = new AutoResetEvent(false);
      eventReceivedNotifier.OnEventReceived += (s, e) =>
      {
        are.Set();
      };
      var services = new ServiceCollection();
      services.AddSingleton<EventReceivedNotifier>(eventReceivedNotifier);
      services.AddScoped<EventPublisherEventHandler>();
      services.AddScoped<SubscriptionTestEventHandler>();
      services.AddInMemoryEventBus();
      services.AddSingleton<ILogger>(new NullLoggerFactory().CreateLogger("logger"));
      var serviceProvider = new DefaultServiceProviderFactory().CreateServiceProvider(services);
      var eventBus = serviceProvider.GetRequiredService<IEventBus>();
      eventBus.Subscribe<EventPublisherEvent, EventPublisherEventHandler>();
      eventBus.Subscribe<SubscriptionTestEvent, SubscriptionTestEventHandler>();
      var eventPublisherEvent = new EventPublisherEvent();
      eventBus.Publish(eventPublisherEvent);
      var eventReceived = are.WaitOne(5000);
      Assert.True(eventReceived);
      Assert.NotEmpty(eventReceivedNotifier.Events);
    }
  }
}