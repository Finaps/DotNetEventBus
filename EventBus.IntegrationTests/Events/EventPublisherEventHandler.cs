using System.Threading.Tasks;
using Finaps.EventBus.Core.Abstractions;
using Finaps.EventBus.Core.Events;

namespace Finaps.EventBus.IntegrationTests.Events
{
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
      _eventBus.Publish(publishEvent);
      return Task.CompletedTask;
    }


  }
}