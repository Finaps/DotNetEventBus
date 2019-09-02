using System;
using Finaps.EventBus.Core;

namespace Finaps.EventBus.Core.Abstractions
{
  public interface IEventSubscriber
  {
    event EventHandler<IntegrationEventReceivedArgs> OnEventReceived;
    void Subscribe(string eventName);

  }
}