using System;
using System.Collections.Generic;
using Finaps.EventBus.Core.Abstractions;
using Finaps.EventBus.Core.Events;

namespace Finaps.EventBus.Core
{
  public interface IEventBusSubscriptionsManager
  {
    bool IsEmpty { get; }
    event EventHandler<string> OnEventRemoved;

    void AddSubscription<T, TH>(TH handler)
       where T : IntegrationEvent
       where TH : IIntegrationEventHandler<T>;

    bool HasSubscriptionsForEvent<T>() where T : IntegrationEvent;
    bool HasSubscriptionsForEvent(string eventName);
    Type GetEventTypeByName(string eventName);
    void Clear();
    IEnumerable<IIntegrationEventHandler> GetHandlersForEvent<T>() where T : IntegrationEvent;
    IEnumerable<IIntegrationEventHandler> GetHandlersForEvent(string eventName);
    string GetEventKey<T>();
  }
}