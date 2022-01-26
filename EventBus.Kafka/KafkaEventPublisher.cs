using Confluent.Kafka;
using Finaps.EventBus.Core.Abstractions;
using Finaps.EventBus.Kafka.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Kafka
{
  public class KafkaEventPublisher : IEventPublisher
  {
    // private bool _disposed;
    private readonly ILogger<KafkaEventPublisher> _logger;
    private KafkaOptions _options;
    private IProducer<int, string> _producer;
    internal KafkaEventPublisher(
      ILogger<KafkaEventPublisher> logger,
      KafkaOptions options
    )
    {
      _logger = logger;
      _options = options;
      _producer = new ProducerBuilder<int, string>(
        new ProducerConfig{ BootstrapServers = options.Brokers }
      ).Build(); //Todo Get it from pool here
    }
    public void Publish(string message, string eventName, string messageId)
    {
      _producer.Produce("test", CreateMessage(message, eventName));
    }
    public Task PublishAsync(string message, string eventName, string messageId)
    {
      return _producer.ProduceAsync("test", CreateMessage(message, eventName));
    }
    private Message<int, string> CreateMessage(string message, string eventName)
    {
      var headers = new Headers();
      headers.Add("eventName", Encoding.ASCII.GetBytes(eventName) );
      var key = new Random().Next(9999);
      return new Message<int, string> { Headers = headers, Key=key, Value = message };
    }

    public ValueTask DisposeAsync()
    {
      return default;
    }
  }
}