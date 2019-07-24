using System;
using System.Collections.Generic;
using System.Linq;
using Finaps.EventBus.Core.Abstractions;
using Finaps.EventBus.Core.Events;

namespace Finaps.EventBus.Core
{
  public partial class InMemoryEventBusSubscriptionsManager : IEventBusSubscriptionsManager
  {


    private readonly Dictionary<string, List<IIntegrationEventHandler>> _handlers;
    private readonly List<Type> _eventTypes;

    public event EventHandler<string> OnEventRemoved;

    public InMemoryEventBusSubscriptionsManager()
    {
      _handlers = new Dictionary<string, List<IIntegrationEventHandler>>();
      _eventTypes = new List<Type>();
    }

    public bool IsEmpty => !_handlers.Keys.Any();
    public void Clear() => _handlers.Clear();

    public void AddSubscription<T, TH>(TH handler)
        where T : IntegrationEvent
        where TH : IIntegrationEventHandler<T>
    {
      var eventName = GetEventKey<T>();

      DoAddSubscription(handler, eventName);

      if (!_eventTypes.Contains(typeof(T)))
      {
        _eventTypes.Add(typeof(T));
      }
    }

    private void DoAddSubscription(IIntegrationEventHandler handler, string eventName)
    {
      if (!HasSubscriptionsForEvent(eventName))
      {
        _handlers.Add(eventName, new List<IIntegrationEventHandler>());
      }

      if (_handlers[eventName].Any(existingHandler => existingHandler == handler))
      {
        throw new ArgumentException(
            $"Handler Type {handler.GetType()} already registered for '{eventName}'", nameof(handler));
      }

      _handlers[eventName].Add(handler);

    }

    public IEnumerable<IIntegrationEventHandler> GetHandlersForEvent<T>() where T : IntegrationEvent
    {
      var key = GetEventKey<T>();
      return GetHandlersForEvent(key);
    }
    public IEnumerable<IIntegrationEventHandler> GetHandlersForEvent(string eventName) => _handlers[eventName];

    public bool HasSubscriptionsForEvent<T>() where T : IntegrationEvent
    {
      var key = GetEventKey<T>();
      return HasSubscriptionsForEvent(key);
    }
    public bool HasSubscriptionsForEvent(string eventName) => _handlers.ContainsKey(eventName);

    public Type GetEventTypeByName(string eventName) => _eventTypes.SingleOrDefault(t => t.Name == eventName);

    public string GetEventKey<T>()
    {
      return typeof(T).Name;
    }
  }
}