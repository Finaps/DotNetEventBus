using Finaps.EventBus.Core.Models;
namespace Finaps.EventBus.IntegrationTests.Events;
public class SubscriptionTestEvent : IntegrationEvent
{
  public string TestString { get; set; }
}