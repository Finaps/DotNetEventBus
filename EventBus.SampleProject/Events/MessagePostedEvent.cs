using Finaps.EventBus.Core.Models;

namespace EventBus.SampleProject.Events
{
  public class MessagePostedEvent : IntegrationEvent
  {
    public string Message { get; set; }
  }
}