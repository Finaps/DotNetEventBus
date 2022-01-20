using System;
using System.Threading.Tasks;
using Finaps.EventBus.Core.Utilities;
namespace Finaps.EventBus.Core.Abstractions
{

  public interface IEventSubscriber : IAsyncDisposable
  {
    Task InitializeAsync();
    event AsyncEventHandler<IntegrationEventReceivedArgs> OnEventReceived;
    Task SubscribeAsync(string eventName);
    Task StartConsumingAsync();
  }
}