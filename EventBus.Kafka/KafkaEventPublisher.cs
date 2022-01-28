using Confluent.Kafka;
using Finaps.EventBus.Core.Abstractions;
using Finaps.EventBus.Kafka.Configuration;
using Finaps.EventBus.Kafka.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EventBus.Kafka
{
  public class KafkaEventPublisher : IEventPublisher
  {
    // private bool _disposed;
    private readonly ILogger<KafkaEventPublisher> _logger;
    private KafkaOptions _options;
    private ObjectPool<IProducer<int, string>> _channelPool;
    internal KafkaEventPublisher(
      ILogger<KafkaEventPublisher> logger,
      KafkaOptions options
    )
    {
      _logger = logger;
      _options = options;
      _channelPool = CreateChannelPool();
    }
    
    public void Publish(string message, string eventName, string messageId)
    {
      string topic = GetTopicFromKafkaMessage(message);
      IProducer<int, string> channel = _channelPool.Get();
      channel.Produce(topic, CreateMessage(message, eventName));
      _channelPool.Return(channel);
    }

    public Task PublishAsync(string message, string eventName, string messageId)
    {
      string topic = GetTopicFromKafkaMessage(message);
      IProducer<int, string> channel = _channelPool.Get();
      channel.ProduceAsync(topic, CreateMessage(message, eventName));
      _channelPool.Return(channel);
      return Task.CompletedTask;
    }

    public string GetTopicFromKafkaMessage(string message)
    {
      var kafkaMessage = JsonSerializer.Deserialize<KafkaMessageEvent>(message);
      string topic;
      if(kafkaMessage != null){
        topic = kafkaMessage.Topic;
      }
      else{
        throw new Exception("Invalid kafka message format");
      }
      if(topic == null){
        throw new Exception("Topic not set in kafka message");
      }
      return topic;
    }

    private Message<int, string> CreateMessage(string message, string eventName)
    {
      var headers = new Headers();
      headers.Add("eventName", Encoding.ASCII.GetBytes(eventName) );
      var key = new Random().Next(9999);
      return new Message<int, string> { Headers = headers, Key=key, Value = message };
    }

    private ObjectPool<IProducer<int, string>> CreateChannelPool()
    {
      var channelPoolPolicy = new ChannelPoolPolicy(_options.Brokers ??= "");
      var channelPoolProvider = new DefaultObjectPoolProvider()
      {
        MaximumRetained = 10
      };
      return channelPoolProvider.Create(channelPoolPolicy);
    }

    private class ChannelPoolPolicy : PooledObjectPolicy<IProducer<int, string>>
    {
      private readonly ProducerConfig _producerConfig;

      public ChannelPoolPolicy(string brokers)
      {
        this._producerConfig = new ProducerConfig{ BootstrapServers = brokers };
      }

      public override IProducer<int, string> Create()
      {
        var channel = new ProducerBuilder<int, string>(_producerConfig).Build();
        return channel;
      }

      public override bool Return(IProducer<int, string> obj)
      {
        return obj != null;
      }
    }

    public ValueTask DisposeAsync()
    {
      return default;
    }
  }
}