using System;
using System.Collections.Generic;
using Finaps.EventBus.Core;
using Finaps.EventBus.Core.Abstractions;

namespace EventBus.Core
{
  public class InMemoryEventBusConnection : IEventBusConnection
  {
    private List<string> subscriptions = new List<string>();
    public bool IsConnected => true;

    public event EventHandler<IntegrationEventReceivedArgs> OnEventReceived;

    public void Publish(string eventName, string message)
    {
      if (subscriptions.Contains(eventName))
      {
        var eventArgs = new IntegrationEventReceivedArgs(eventName, message);
        OnEventReceived(this, eventArgs);
      }
    }

    public void Subscribe(string eventName)
    {
      subscriptions.Add(eventName);
    }

    public bool TryConnect()
    {
      return true;
    }
  }
}