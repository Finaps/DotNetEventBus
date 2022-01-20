

using Finaps.EventBus.Core.Models;
namespace EventBus.SampleProject.Events
{
  public class MessagePutEvent : IntegrationEvent
  {
    public string Message { get; set; }
  }
}