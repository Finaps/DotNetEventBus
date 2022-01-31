using EventBus.IntegrationTests;
using EventBus.IntegrationTests.Events;
using Finaps.EventBus.Core.Abstractions;
using Finaps.EventBus.Core.DependencyInjection;
using Finaps.EventBus.IntegrationTests.Events;
using Finaps.EventBus.Kafka.Configuration;
using Finaps.EventBus.Kafka.Extensions;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Finaps.EventBus.IntegrationTests;

[Collection("Sequential")]
public class KafaEventBusTests : IDisposable
{
  private static readonly int ConsumeTimeoutInMilliSeconds = 15000;
  protected EventReceivedNotifier eventReceivedNotifier;
  protected IntegerIncrementer integerIncrementer;
  protected AutoResetEvent autoResetEvent;
  protected IEventBus eventBus;
  public KafaEventBusTests()
  {
    eventReceivedNotifier = new EventReceivedNotifier();
    integerIncrementer = new IntegerIncrementer();
    autoResetEvent = new AutoResetEvent(false);
    eventReceivedNotifier.OnEventReceived += (s, e) =>
    {
      autoResetEvent.Set();
    };
    var services = SetupServices();
    eventBus = SetupEventBus(services);
  }

  private ServiceCollection SetupServices()
  {
    var services = new ServiceCollection();
    services.AddSingleton<EventReceivedNotifier>(eventReceivedNotifier);
    services.AddSingleton<IntegerIncrementer>(integerIncrementer);
    services.AddSingleton<EventBusStartup>();
    services.AddScoped<EventPublisherEventHandler>();
    services.AddScoped<KafkaTestEventHandler>();
    services.AddScoped<CheckConcurrencyEventHandler>();
    services.AddSingleton<ILoggerFactory>(new NullLoggerFactory());
    services.AddLogging();
    return services;
  }

  private static IEventBus SetupEventBus(ServiceCollection services)
  {
    services.ConfigureKafka(config =>
    {
      config.Options = new KafkaOptions
      {
        Brokers = "localhost:9094",
        TopicNames = new string[] { "test","test2" },
        GroupId = "test_group"
      };
      SetupSubscriptions(config);
    });

    var serviceProvider = new DefaultServiceProviderFactory().CreateServiceProvider(services);
    var backgroundConsumer = serviceProvider.GetRequiredService<EventBusStartup>();
    Task.Run(() => backgroundConsumer.StartAsync(CancellationToken.None)).Wait();
    var eventBus = serviceProvider.GetRequiredService<IEventBus>();
    return eventBus;
  }

  protected static void SetupSubscriptions(BaseEventBusConfiguration config)
  {
    config.AddSubscription<EventPublisherEvent, EventPublisherEventHandler>();
    config.AddSubscription<KafkaTestEvent, KafkaTestEventHandler>();
    config.AddSubscription<CheckConcurrencyEvent, CheckConcurrencyEventHandler>();
  }

  protected KafkaTestEvent PublishKafkaTestEvent()
  {
    var kafkaTestEvent = new KafkaTestEvent()
    {
      Message = "testMessage",
      Topic = "test",
    };
    eventBus.PublishAsync(kafkaTestEvent);
    return kafkaTestEvent;
  }

  public void Dispose()
  {
    autoResetEvent.Dispose();
    eventBus.DisposeAsync();
  }

  [Fact]
  public void ListensCorrectly()
  {
    var kafkaTestEvent = PublishKafkaTestEvent();
    var eventReceived = autoResetEvent.WaitOne(ConsumeTimeoutInMilliSeconds);
    Assert.True(eventReceived);
    var consumedEvent = eventReceivedNotifier.Events.Single() as KafkaTestEvent;
    Assert.Equal(kafkaTestEvent.Message, consumedEvent.Message);
    Assert.Equal(kafkaTestEvent.Id, consumedEvent.Id);
    Assert.Equal(kafkaTestEvent.CreationDate, consumedEvent.CreationDate);
  }

  [Fact]
  public void CanPublishWhileReceiving()
  {
    var eventPublisherEvent = PublishKafkaTestEvent();
    var eventReceived = autoResetEvent.WaitOne(ConsumeTimeoutInMilliSeconds);
    Assert.True(eventReceived);
    Assert.NotEmpty(eventReceivedNotifier.Events);
  }

  [Fact]
  public void EventsAreReceivedInOrder()
  {
    var publishedEvents = new List<KafkaTestEvent>();
    for (int i = 0; i < 50; i++)
    {
      publishedEvents.Add(PublishKafkaTestEvent());
    }
    var eventReceived = autoResetEvent.WaitOne(ConsumeTimeoutInMilliSeconds);
    Assert.True(eventReceived);
    var publishedGuids = publishedEvents.Select(@event => @event.Id);
    var consumedGuids = eventReceivedNotifier.Events.Select(@event => @event.Id);
    Assert.Equal(publishedGuids, consumedGuids);
  }

  [Fact]
  public async Task IncorrectMessageFormatThrowsError()
  {
    await Assert.ThrowsAsync<Exception>(async () => await eventBus.PublishAsync(new CheckConcurrencyEvent()));
  }
}
