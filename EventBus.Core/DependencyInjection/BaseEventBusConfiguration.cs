using System.Collections.Generic;
using Finaps.EventBus.Core.Abstractions;
using Finaps.EventBus.Core.Models;

namespace Finaps.EventBus.Core.DependencyInjection
{

  public abstract class BaseEventBusConfiguration
  {
    public List<EventSubscription> Subscriptions { get; } = new List<EventSubscription>();

    public void AddSubscription<T, TH>()
      where T : IntegrationEvent
      where TH : IIntegrationEventHandler<T>
    {
      Subscriptions.Add(new EventSubscription(typeof(T), typeof(TH)));
    }
  }
}