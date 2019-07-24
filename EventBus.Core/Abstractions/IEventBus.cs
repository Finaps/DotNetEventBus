using Finaps.EventBus.Core.Events;

namespace Finaps.EventBus.Core.Abstractions
{
  public interface IEventBus
  {
    void Publish(IntegrationEvent @event);

    void Subscribe<T, TH>(TH handler)
        where T : IntegrationEvent
        where TH : IIntegrationEventHandler<T>;
  }
}