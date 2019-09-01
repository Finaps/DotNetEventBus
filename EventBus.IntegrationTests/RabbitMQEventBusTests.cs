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
  public class RabbitMQEventBusTests
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
      services.AddSingleton(new NullLoggerFactory());
      services.AddLogging();
      services.AddRabbitMQ(new RabbitMQOptions()
      {
        ExchangeName = "Exchange",
        QueueName = "IntegrationTests",
        UserName = "guest",
        Password = "guest"
      });
      var serviceProvider = new DefaultServiceProviderFactory().CreateServiceProvider(services);
      var eventBus = serviceProvider.GetRequiredService<IEventBus>();
      eventBus.Subscribe<SubscriptionTestEvent, SubscriptionTestEventHandler>();
      string testString = "test";
      var subscriptionTestEvent = new SubscriptionTestEvent()
      {
        TestString = testString
      };
      eventBus.Publish(subscriptionTestEvent);
      var eventReceived = are.WaitOne(20000);
      Assert.True(eventReceived);
      var consumedEvent = eventReceivedNotifier.Events.Single() as SubscriptionTestEvent;
      Assert.Equal(testString, consumedEvent.TestString);
      Assert.Equal(subscriptionTestEvent.Id, consumedEvent.Id);
      Assert.Equal(subscriptionTestEvent.CreationDate, consumedEvent.CreationDate);

    }

    [Fact]
    public void CanCreateConnection()
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
      var eventPublisherEvent = new EventPublisherEvent();
      eventBus.Publish(eventPublisherEvent);
      var eventReceived = are.WaitOne(20000);
      Assert.True(eventReceived);
      Assert.NotEmpty(eventReceivedNotifier.Events);
    }
  }
}
