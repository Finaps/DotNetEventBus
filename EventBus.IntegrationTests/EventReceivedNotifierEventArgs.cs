using Finaps.EventBus.Core.Models;

namespace Finaps.EventBus.IntegrationTests
{
  public class EventReceivedNotifierEventArgs
  {
    public IntegrationEvent Event { get; set; }
  }
}