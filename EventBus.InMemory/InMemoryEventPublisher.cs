using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using Finaps.EventBus.Core.Abstractions;
using Finaps.EventBus.Core.Events;

namespace Finaps.EventBus.InMemory
{
  public class InMemoryEventPublisher : IEventPublisher
  {

    private readonly BlockingCollection<IntegrationEvent> _events;

    public InMemoryEventPublisher(BlockingCollection<IntegrationEvent> events)
    {
      _events = events;
    }

    public void Dispose()
    {
      _events.CompleteAdding();
    }

    public void Publish(IntegrationEvent @event)
    {
      _events.Add(@event);
    }
  }
}