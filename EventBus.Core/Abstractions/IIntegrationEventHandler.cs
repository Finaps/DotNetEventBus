using System.Threading.Tasks;
using Finaps.EventBus.Core.Models;
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