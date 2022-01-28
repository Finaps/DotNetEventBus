using EventBus.IntegrationTests;
using EventBus.IntegrationTests.Events;
using Finaps.EventBus.Core.Abstractions;
using Finaps.EventBus.Core.DependencyInjection;
using Finaps.EventBus.IntegrationTests.Events;
using Finaps.EventBus.RabbitMq.Configuration;
using Finaps.EventBus.RabbitMq.Extensions;
using Finaps.EventBus.AzureServiceBus.Extensions;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
namespace Finaps.EventBus.IntegrationTests;

[Collection("Sequential")]
public class BaseEventBusTests : IDisposable
{
  private static readonly int ConsumeTimeoutInMilliSeconds = 15000;
  protected EventReceivedNotifier eventReceivedNotifier;
  protected IntegerIncrementer integerIncrementer;
  protected AutoResetEvent autoResetEvent;
  protected IEventBus eventBus;
  public BaseEventBusTests()
  {
    var eventBusType = EventBusType.RabbitMQ;
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
    services.AddSingleton<EventBusStartup>();
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
        services.ConfigureRabbitMq(config =>
        {
          config.Options = new RabbitMqOptions
          {
            ExchangeName = "SampleProject",
            QueueName = "SampleProject",
            UserName = "guest",
            Password = "guest",
            VirtualHost = "/"
          };
          SetupSubscriptions(config);
        });
        break;
      case EventBusType.Azure:
        services.ConfigureAzureServiceBus(config =>
        {
          config.Options.ConnectionString = "Endpoint=sb://finaps-bus.servicebus.windows.net/;SharedAccessKeyName=IntegrationTest;SharedAccessKey=x55a0K04JdGS+5/uFNTw99raxFFUY5T7iq2UFBbZbBg=;EntityPath=test";
          config.Options.SubscriptionName = "IntegrationTest";
          config.Options.TopicName = "topic";
          SetupSubscriptions(config);
        });
        break;
    }


    var serviceProvider = new DefaultServiceProviderFactory().CreateServiceProvider(services);
    var backgroundConsumer = serviceProvider.GetRequiredService<EventBusStartup>();
    Task.Run(() => backgroundConsumer.StartAsync(CancellationToken.None)).Wait();
    var eventBus = serviceProvider.GetRequiredService<IEventBus>();
    return eventBus;
  }

  protected static void SetupSubscriptions(BaseEventBusConfiguration config)
  {
    config.AddSubscription<EventPublisherEvent, EventPublisherEventHandler>();
    config.AddSubscription<SubscriptionTestEvent, SubscriptionTestEventHandler>();
    config.AddSubscription<CheckConcurrencyEvent, CheckConcurrencyEventHandler>();
  }

  protected SubscriptionTestEvent PublishSubscriptionTestEvent()
  {
    var subscriptionTestEvent = new SubscriptionTestEvent()
    {
      TestString = "test"
    };
    eventBus.PublishAsync(subscriptionTestEvent);
    return subscriptionTestEvent;
  }

  public void Dispose()
  {
    autoResetEvent.Dispose();
    eventBus.DisposeAsync();
  }

  [Fact]
  public void ListensCorrectly()
  {
    var subscriptionTestEvent = PublishSubscriptionTestEvent();
    var eventReceived = autoResetEvent.WaitOne(ConsumeTimeoutInMilliSeconds);
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
    eventBus.PublishAsync(eventPublisherEvent);
    var eventReceived = autoResetEvent.WaitOne(ConsumeTimeoutInMilliSeconds);
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
    var eventReceived = autoResetEvent.WaitOne(ConsumeTimeoutInMilliSeconds);
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
      await eventBus.PublishAsync(new CheckConcurrencyEvent());
    }
    await Task.Delay(4000);
    Assert.Equal(10, integerIncrementer.TimesIncremented);
  }
}
