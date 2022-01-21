using System;
using System.Collections.Generic;
using System.Linq;
using Finaps.EventBus.Core.Abstractions;
using Finaps.EventBus.Core.Models;
using Finaps.EventBus.Core.Utilities;

namespace Finaps.EventBus.Core
{
  public class InMemoryEventBusSubscriptionsManager : IEventBusSubscriptionsManager
  {
    private readonly Dictionary<string, List<Type>> _handlers;
    private readonly List<Type> _eventTypes;

    public InMemoryEventBusSubscriptionsManager()
    {
      _handlers = new Dictionary<string, List<Type>>();
      _eventTypes = new List<Type>();
    }

    public bool IsEmpty => !_handlers.Keys.Any();
    public void Clear() => _handlers.Clear();

    public void AddSubscription<T, TH>()
        where T : IntegrationEvent
        where TH : IIntegrationEventHandler<T>
    {
      var eventName = EventTypeUtilities.GetEventKey<T>();

      Subscribe(typeof(TH), eventName);

      if (!_eventTypes.Contains(typeof(T)))
      {
        _eventTypes.Add(typeof(T));
      }
    }

    public void AddSubscription(EventSubscription subscription)
    {

      var eventName = EventTypeUtilities.GetEventKey(subscription.EventType);

      Subscribe(subscription.HandlerType, eventName);

      if (!_eventTypes.Contains(subscription.EventType))
      {
        _eventTypes.Add(subscription.EventType);
      }
    }

    private void Subscribe(Type handlerType, string eventName)
    {
      if (!HasSubscriptionsForEvent(eventName))
      {
        _handlers.Add(eventName, new List<Type>());
      }

      if (_handlers[eventName].Any(existingHandler => existingHandler == handlerType))
      {
        throw new ArgumentException(
            $"Handler Type {handlerType.GetType()} already registered for '{eventName}'", nameof(handlerType));
      }

      _handlers[eventName].Add(handlerType);

    }

    public IEnumerable<Type> GetHandlersForEvent<T>() where T : IntegrationEvent
    {
      var key = EventTypeUtilities.GetEventKey<T>();
      return GetHandlersForEvent(key);
    }
    public IEnumerable<Type> GetHandlersForEvent(string eventName) => _handlers[eventName];

    public bool HasSubscriptionsForEvent<T>() where T : IntegrationEvent
    {
      var key = EventTypeUtilities.GetEventKey<T>();
      return HasSubscriptionsForEvent(key);
    }
    public bool HasSubscriptionsForEvent(string eventName) => _handlers.ContainsKey(eventName);

    public Type GetEventTypeByName(string eventName) => _eventTypes.SingleOrDefault(t => t.Name == eventName);

    public IEnumerable<string> GetSubscriptions()
    {
      return _eventTypes.Select(type => type.Name);
    }
  }
}