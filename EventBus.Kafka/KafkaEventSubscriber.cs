
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Finaps.EventBus.Kafka.Configuration;
using Finaps.EventBus.Core;
using Finaps.EventBus.Core.Abstractions;
using Finaps.EventBus.Core.Utilities;
using Microsoft.Extensions.Logging;

namespace Finaps.EventBus.Kafka
{

  internal class KafkaEventSubscriber : IEventSubscriber
  {
    private bool _disposed;
    private readonly ILogger<KafkaEventSubscriber> _logger;
    private readonly ConsumerConfig config = new ConsumerConfig
    {
      GroupId = "st_consumer_group",
      BootstrapServers = "localhost:9094",
      AutoOffsetReset = AutoOffsetReset.Earliest
    };

    public event AsyncEventHandler<IntegrationEventReceivedArgs> OnEventReceived;

    private IConsumer<Ignore, string> _consumer;

    internal KafkaEventSubscriber(
      KafkaOptions options,
      ILogger<KafkaEventSubscriber> logger)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public Task InitializeAsync()
    {
      return Task.CompletedTask;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
      var conf = new ConsumerConfig
      {
          GroupId = "st_consumer_group",
          BootstrapServers = "localhost:9094",
          AutoOffsetReset = AutoOffsetReset.Earliest
      };
      using (var builder = new ConsumerBuilder<Ignore, 
          string>(conf).Build())
      {
          builder.Subscribe("test");
          var cancelToken = new CancellationTokenSource();
          try
          {
              while (true)
              {
                  var consumer = builder.Consume(cancelToken.Token);
                  Console.WriteLine($"Message: {consumer.Message.Value} received from {consumer.TopicPartitionOffset}");
              }
          }
          catch (Exception e)
          {
              Console.WriteLine($"Something went wrong: {e}");
          }
          finally
          {
              builder.Close();
          }
      }
      return Task.CompletedTask;
    }

    // public async Task SubscribeAsync(string eventName)
    // {
    //   if (!_initialized) throw new InvalidOperationException("Subscriber has not been initialized");
    //   _logger.LogInformation("Subscribing to event {EventName}", eventName);
    //   if (_rules.Any(r => r.Name == eventName)) return;
    //   if (!await _administrationClient.RuleExistsAsync(_options.TopicName, _options.SubscriptionName, eventName))
    //   {
    //     var rule = new CreateRuleOptions(eventName, new CorrelationRuleFilter()
    //     {
    //       Subject = eventName
    //     });
    //     await _administrationClient.CreateRuleAsync(_options.TopicName, _options.SubscriptionName, rule);
    //   }
    // }

    // public async Task StartConsumingAsync()
    // {
    //   if (!_initialized) throw new InvalidOperationException("Subscriber has not been initialized");
    //   _processor = _client.CreateProcessor(_options.TopicName, _options.SubscriptionName, new ServiceBusProcessorOptions()
    //   {
    //     PrefetchCount = 250,
    //     AutoCompleteMessages = false
    //   });
    //   _processor.ProcessMessageAsync += ProcessorOnProcessMessageAsync;
    //   _processor.ProcessErrorAsync += ProcessorOnProcessErrorAsync;
    //   await _processor.StartProcessingAsync();
    // }

    public async ValueTask DisposeAsync()
    {
      if (_disposed) return;
      _disposed = true;
      if (_processor != null) await _processor.DisposeAsync();
      if (_client != null) await _client.DisposeAsync();
    }
  }
}
