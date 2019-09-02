using System;
using Finaps.EventBus.Core.Events;

namespace Finaps.EventBus.Core.Abstractions
{
  public interface IEventPublisher : IDisposable
  {
    void Publish(IntegrationEvent @event);
  }
}