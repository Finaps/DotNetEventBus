using Finaps.EventBus.Kafka.Configuration;
using Finaps.EventBus.Core.DependencyInjection;

namespace Finaps.EventBus.Kafka.DependencyInjection
{

  public class KafkaEventBusConfiguration : BaseEventBusConfiguration
  {
    public KafkaOptions Options { get; set; } = new KafkaOptions();
  }
}