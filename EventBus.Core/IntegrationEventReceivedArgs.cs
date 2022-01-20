using System;

namespace Finaps.EventBus.Core
{
  public class IntegrationEventReceivedArgs : EventArgs
  {

    public string? EventName { get; set; }
    public string? Message { get; set; }
    public IntegrationEventReceivedArgs() { }
    public IntegrationEventReceivedArgs(string? eventName, string? message)
    {
      EventName = eventName;
      Message = message;
    }
  }
}