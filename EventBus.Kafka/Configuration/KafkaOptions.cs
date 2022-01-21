namespace Finaps.EventBus.Kafka.Configuration
{
  public class KafkaOptions
  {
    public string Brokers { get; set; }
    public string TopicName { get; set; }
    public string GroupId { get; set; }
    public string AutoOffsetReset { get; set; }
    public string SubscriptionName { get; set; }
  }
}