using System;
using System.Threading.Tasks;

namespace Finaps.EventBus.Core.Abstractions
{
  public interface IEventPublisher : IAsyncDisposable
  {
    Task PublishAsync(string message, string eventName, string messageId);
  }
}