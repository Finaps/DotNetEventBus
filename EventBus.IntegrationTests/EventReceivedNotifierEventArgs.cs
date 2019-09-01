using Finaps.EventBus.Core.Events;

namespace Finaps.EventBus.IntegrationTests
{
  public class EventReceivedNotifierEventArgs
  {
    public IntegrationEvent Event { get; set; }
  }
}