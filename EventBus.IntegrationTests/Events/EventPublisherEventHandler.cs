using Finaps.EventBus.Core.Abstractions;

namespace Finaps.EventBus.IntegrationTests.Events;
public class EventPublisherEventHandler : IIntegrationEventHandler<EventPublisherEvent>
{
  private readonly IEventBus _eventBus;
  public EventPublisherEventHandler(IEventBus eventBus)
  {
    _eventBus = eventBus;
  }
  public Task Handle(EventPublisherEvent @event)
  {
    var publishEvent = new SubscriptionTestEvent();
    return _eventBus.PublishAsync(publishEvent);
  }
}