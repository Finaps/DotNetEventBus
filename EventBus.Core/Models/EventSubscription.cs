using System;
using Finaps.EventBus.Core.Abstractions;

namespace Finaps.EventBus.Core.Models
{

  public class EventSubscription
  {
    public Type EventType { get; }
    public Type HandlerType { get; }

    public EventSubscription(Type eventType, Type handlerType)
    {
      if (!typeof(IntegrationEvent).IsAssignableFrom(eventType))
      {
        throw new ArgumentException($"Provided type needs to extend {nameof(IntegrationEvent)}", nameof(eventType));
      }
      var genericHandlerType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
      if (!genericHandlerType.IsAssignableFrom(handlerType))
      {
        throw new ArgumentException($"Provided type needs to implement {nameof(IIntegrationEventHandler)}", nameof(handlerType));
      }

      EventType = eventType;
      HandlerType = handlerType;
    }
  }
}
