using System;

namespace Finaps.EventBus.Core.Abstractions
{
  public interface IEventBusConnection
  {
    bool IsConnected { get; }
    bool TryConnect();
    event EventHandler<IntegrationEventReceivedArgs> OnEventReceived;
    void Publish(string eventName, string message);
    void Subscribe(string eventName);
  }
}