using Finaps.EventBus.Core.Events;

namespace EventBus.SampleProject.Events
{
  public class MessagePutEvent : IntegrationEvent
  {
    public string Message { get; set; }
  }
}