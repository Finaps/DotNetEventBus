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
      return services.AddSingleton<IEventBus>(sp =>
      {
        var events = new ObservableCollection<IntegrationEvent>();
        var publisher = new InMemoryEventPublisher(events);
        var subscriber = new InMemoryEventSubscriber(events);
        var subscriptionManager = new InMemoryEventBusSubscriptionsManager();
        var logger = sp.GetService<ILogger>();
        return new DefaultEventBus(publisher, subscriber, subscriptionManager, sp, logger);
      });
    }
  }
}