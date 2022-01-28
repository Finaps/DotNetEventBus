
using System;
using System.Threading.Tasks;
using Finaps.EventBus.Kafka.Configuration;
using Finaps.EventBus.Core;
using Finaps.EventBus.Core.Abstractions;
using Finaps.EventBus.Core.Utilities;
using Microsoft.Extensions.Logging;
using Confluent.Kafka;
using System.Text;
using System.Collections.Generic;

namespace Finaps.EventBus.Kafka
{

  internal class KafkaEventSubscriber : IEventSubscriber
  {
    private bool _disposed;
    private readonly ILogger<KafkaEventSubscriber> _logger;
    public event AsyncEventHandler<IntegrationEventReceivedArgs>? OnEventReceived;
    private KafkaOptions _options;
    private IConsumer<Ignore, string> _consumer;

    internal KafkaEventSubscriber(
      ILogger<KafkaEventSubscriber> logger,
      KafkaOptions options
    )
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _options = options;
      _consumer = new ConsumerBuilder<Ignore, string>(
        new ConsumerConfig
        {
          GroupId = options.GroupId,
          BootstrapServers = options.Brokers,
          AutoOffsetReset = AutoOffsetReset.Earliest,
        }
      ).Build();

      //TODO: Make sure all topics exist, error if trying to subscribe to topic thats not
      _consumer.Subscribe(options.TopicNames);
    }

    public Task InitializeAsync()
    {
      return Task.CompletedTask;
    }

    public Task SubscribeAsync(string eventName)
    {
      return Task.CompletedTask;
    }

    public Task StartConsumingAsync()
    {
      Task.Run(async () => {
        try
        {
          while (true)
          {
              var consumer = _consumer.Consume();
              _logger.LogInformation($"Message: {consumer.Message.Value} received from {consumer.TopicPartitionOffset}");
              await OnMessageReceived(consumer);
          }
        }
        catch (Exception e)
        {
            _logger.LogTrace($"Something went wrong: {e}");
        }
        finally
        {
          await DisposeAsync();
        }
      });
      return Task.CompletedTask;
    }

    public async Task OnMessageReceived(ConsumeResult<Ignore, string> consumer)
    {
      var bytes = consumer.Message.Headers[0].GetValueBytes();

      var integrationEventReceivedArgs = new IntegrationEventReceivedArgs()
      {
        EventName = Encoding.UTF8.GetString(bytes, 0, bytes.Length),
        Message = consumer.Message.Value
      };

      if (OnEventReceived != null){
        await Task.Run(async () => {
          await OnEventReceived.Invoke(this, integrationEventReceivedArgs);
        });
      }
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
