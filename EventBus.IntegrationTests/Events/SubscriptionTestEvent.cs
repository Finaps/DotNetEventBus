using Finaps.EventBus.Core.Events;

namespace Finaps.EventBus.IntegrationTests.Events
{
  public class SubscriptionTestEvent : IntegrationEvent
  {
    public string TestString { get; set; }
  }
}