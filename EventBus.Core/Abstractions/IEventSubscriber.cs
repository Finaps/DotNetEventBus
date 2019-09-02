using System;

namespace Finaps.EventBus.Core.Abstractions
{
  public interface IEventSubscriber : IDisposable
  {
    event EventHandler<IntegrationEventReceivedArgs> OnEventReceived;
    void Subscribe(string eventName);

  }
}