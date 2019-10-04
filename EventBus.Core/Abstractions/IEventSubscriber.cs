using System;
using System.Threading.Tasks;

namespace Finaps.EventBus.Core.Abstractions
{
  public delegate Task AsyncEventHandler<T>(object sender, T eventReceivedArgs);
  public interface IEventSubscriber : IDisposable
  {
    event AsyncEventHandler<IntegrationEventReceivedArgs> OnEventReceived;
    void Subscribe(string eventName);

  }
}