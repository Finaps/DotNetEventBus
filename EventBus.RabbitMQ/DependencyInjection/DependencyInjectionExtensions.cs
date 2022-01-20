using Finaps.EventBus.Core.DependencyInjection;
using Finaps.EventBus.RabbitMq.Configuration;

namespace Finaps.EventBus.RabbitMq.DependencyInjection
{
  public class RabbitMqEventBusConfiguration : BaseEventBusConfiguration
  {
    public RabbitMqOptions Options { get; set; } = new RabbitMqOptions();
  }
}