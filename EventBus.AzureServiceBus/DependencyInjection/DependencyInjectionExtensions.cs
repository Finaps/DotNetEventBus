using Finaps.EventBus.AzureServiceBus.Configuration;
using Finaps.EventBus.Core.DependencyInjection;

namespace Finaps.EventBus.AzureServiceBus.DependencyInjection
{

  public class AzureServiceBusEventBusConfiguration : BaseEventBusConfiguration
  {
    public AzureServiceBusOptions Options { get; set; } = new AzureServiceBusOptions();
  }
}