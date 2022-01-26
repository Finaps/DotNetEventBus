namespace Finaps.EventBus.Kafka.Configuration
{
  public class KafkaOptions
  {
    public string? Brokers { get; set; }
    public string[]? TopicNames { get; set; }
    public string? GroupId { get; set; }
  }
}