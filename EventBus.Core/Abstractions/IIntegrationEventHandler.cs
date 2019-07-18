using System.Threading.Tasks;
using Finaps.EventBus.Core.Events;

namespace Finaps.EventBus.Core.Abstractions
{
  public interface IIntegrationEventHandler<in TIntegrationEvent> : IIntegrationEventHandler
      where TIntegrationEvent : IntegrationEvent
  {
    Task Handle(TIntegrationEvent @event);
  }

  public interface IIntegrationEventHandler
  {
  }
}