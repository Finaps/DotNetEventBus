
using System;
using System.Threading.Tasks;
using Finaps.EventBus.Kafka.Configuration;
using Finaps.EventBus.Core;
using Finaps.EventBus.Core.Abstractions;
using Finaps.EventBus.Core.Utilities;
using Microsoft.Extensions.Logging;
using Confluent.Kafka;

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
      AutoOffsetReset = AutoOffsetReset.Earliest,
    };

    public event AsyncEventHandler<IntegrationEventReceivedArgs>? OnEventReceived;

    private IConsumer<Ignore, string> _consumer;

    internal KafkaEventSubscriber(
      ILogger<KafkaEventSubscriber> logger)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
    }

    public Task InitializeAsync()
    {
      return Task.CompletedTask;
    }

    public Task SubscribeAsync(string eventName)
    {
      _consumer.Subscribe("test");
      return Task.CompletedTask;
    }

    public Task StartConsumingAsync()
    {
      // var cancelToken = new CancellationTokenSource();
      // try
      // {
        //wrap 
        Task.Run(() => {
          while (true)
          {
              Console.WriteLine($"Consuming...");
              var consumer = _consumer.Consume();
              Console.WriteLine($"Message: {consumer.Message.Value} received from {consumer.TopicPartitionOffset}");
          }
        });
        return Task.CompletedTask;
      // }
      // catch (Exception e)
      // {
      //     Console.WriteLine($"Something went wrong: {e}");
      // }
      // finally
      // {
      //   await DisposeAsync();
      // }
    }

    public ValueTask DisposeAsync()
    {
      if (_disposed) return default;

      _disposed = true;
      _logger.LogTrace($"Disposing {this.GetType().Name}");
      _consumer.Close();
      _logger.LogTrace($"{this.GetType().Name} disposed");

      return default;
    }
  }
}
