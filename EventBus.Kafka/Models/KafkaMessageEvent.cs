using Finaps.EventBus.Core.Models;

namespace Finaps.EventBus.Kafka.Models
{
  public class KafkaMessageEvent : IntegrationEvent
  {
    public string? Message { get; set; }
    public string? Topic { get; set; }
  }
}