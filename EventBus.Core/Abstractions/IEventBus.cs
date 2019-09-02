using System;
using Finaps.EventBus.Core.Events;

namespace Finaps.EventBus.Core.Abstractions
{
  public interface IEventBus : IDisposable
  {
    void Publish(IntegrationEvent @event);

    void Subscribe<T, TH>()
        where T : IntegrationEvent
        where TH : IIntegrationEventHandler<T>;
  }
}