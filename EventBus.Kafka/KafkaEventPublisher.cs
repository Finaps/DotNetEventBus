using Confluent.Kafka;
using Finaps.EventBus.Core.Abstractions;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace EventBus.Kafka
{
  public class KafkaEventPublisher : IEventPublisher
  {
    // private bool _disposed;
    private readonly ILogger<KafkaEventPublisher> _logger;
    private readonly ProducerConfig config = new ProducerConfig
    { BootstrapServers = "localhost:9094" };
    private IProducer<Null, string> _producer;
    internal KafkaEventPublisher(
      ILogger<KafkaEventPublisher> logger
    )
    {
      _logger = logger;
      _producer = new ProducerBuilder<Null, string>(config).Build(); //Todo Get it from pool here
    }
    public void Publish(string message, string eventName, string messageId)
    {
      _producer.Produce(eventName, CreateMessage(message));
    }
    public Task PublishAsync(string message, string eventName, string messageId)
    {
      return _producer.ProduceAsync(eventName, CreateMessage(message));
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