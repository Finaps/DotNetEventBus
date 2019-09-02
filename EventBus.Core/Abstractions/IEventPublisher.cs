using Finaps.EventBus.Core.Events;

namespace Finaps.EventBus.Core.Abstractions
{
  public interface IEventPublisher
  {
    void Publish(IntegrationEvent @event);
  }
}