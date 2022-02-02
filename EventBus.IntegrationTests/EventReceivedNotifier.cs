using Finaps.EventBus.Core.Models;
namespace Finaps.EventBus.IntegrationTests;

public class EventReceivedNotifier
{
  public List<IntegrationEvent> Events { get; } = new List<IntegrationEvent>();
  public void NotifyEventReceived(IntegrationEvent integrationEvent)
  {
    Events.Add(integrationEvent);
    var eventArgs = new EventReceivedNotifierEventArgs()
    {
      Event = integrationEvent
    };
    // Console.WriteLine(Events.Count());
    OnEventReceived?.Invoke(this, eventArgs);
  }

  public event EventHandler<EventReceivedNotifierEventArgs> OnEventReceived;
}
