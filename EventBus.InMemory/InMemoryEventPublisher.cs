using System.Collections.ObjectModel;
using Finaps.EventBus.Core.Abstractions;
using Finaps.EventBus.Core.Events;

namespace Finaps.EventBus.InMemory
{
  public class InMemoryEventPublisher : IEventPublisher
  {

    private readonly ObservableCollection<IntegrationEvent> _events;

    public InMemoryEventPublisher(ObservableCollection<IntegrationEvent> events)
    {
      _events = events;
    }

    public void Publish(IntegrationEvent @event)
    {
      _events.Add(@event);
    }
  }
}