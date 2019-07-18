using System.Threading.Tasks;

namespace Finaps.EventBus.Core.Abstractions
{
  public interface IDynamicIntegrationEventHandler
  {
    Task Handle(dynamic eventData);
  }
}