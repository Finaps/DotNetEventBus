using System.Collections.ObjectModel;
using Finaps.EventBus.Core;
using Finaps.EventBus.Core.Abstractions;
using Finaps.EventBus.Core.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Finaps.EventBus.InMemory.DependencyInjection
{
  public static class DependencyInjectionExtensions
  {
    public static IServiceCollection AddInMemoryEventBus(this IServiceCollection services)
    {
      var events = new ObservableCollection<IntegrationEvent>();
      services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();
      services.AddSingleton<IEventPublisher>(sp =>
      {
        return new InMemoryEventPublisher(
          events
        );
      });
      services.AddSingleton<IEventSubscriber>(sp =>
      {
        return new InMemoryEventSubscriber(
          events
        );
      });

      return services.AddSingleton<IEventBus>(sp =>
      {
        return new DefaultEventBus(
          sp.GetRequiredService<IEventPublisher>(),
          sp.GetRequiredService<IEventSubscriber>(),
          sp.GetRequiredService<IEventBusSubscriptionsManager>(),
          sp,
          sp.GetRequiredService<ILoggerFactory>().CreateLogger("InMemoryEventBus")
          );
      });
    }
  }
}