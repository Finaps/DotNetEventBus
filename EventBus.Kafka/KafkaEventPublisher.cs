using Confluent.Kafka;
using Finaps.EventBus.Core.Abstractions;
using Finaps.EventBus.Kafka.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace EventBus.Kafka
{
  public class KafkaEventPublisher : IEventPublisher
  {
    // private bool _disposed;
    private readonly ILogger<KafkaEventPublisher> _logger;
    private KafkaOptions _options;
    private IProducer<Null, string> _producer;
    internal KafkaEventPublisher(
      ILogger<KafkaEventPublisher> logger,
      KafkaOptions options
    )
    {
      _logger = logger;
      _options = options;
      _producer = new ProducerBuilder<Null, string>(
        new ProducerConfig{ BootstrapServers = options.Brokers }
      ).Build(); //Todo Get it from pool here
    }
    public void Publish(string message, string eventName, string messageId)
    {
      _producer.Produce(_options.TopicName, CreateMessage(message));
    }
    public Task PublishAsync(string message, string eventName, string messageId)
    {
      return _producer.ProduceAsync(_options.TopicName, CreateMessage(message));
    }
    private Message<Null, string> CreateMessage(string message)
    {
      return new Message<Null, string> { Value = message };
    }

    public ValueTask DisposeAsync()
    {
      return default;
    }
  }
}