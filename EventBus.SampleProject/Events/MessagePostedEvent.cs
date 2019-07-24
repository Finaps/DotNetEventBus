using Finaps.EventBus.Core.Events;

namespace EventBus.SampleProject.Events
{
  public class MessagePostedEvent : IntegrationEvent
  {
    public string Message { get; set; }
  }
}