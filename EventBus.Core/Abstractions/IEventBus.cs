
using System;
using System.Threading.Tasks;
using Finaps.EventBus.Core.Models;

namespace Finaps.EventBus.Core.Abstractions
{
  public interface IEventBus : IAsyncDisposable
  {
    Task PublishAsync(IntegrationEvent @event);

    Task StartConsumingAsync();

    void AddSubscription(EventSubscription subscription);
  }
}
