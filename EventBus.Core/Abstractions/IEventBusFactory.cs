namespace Finaps.EventBus.Core.Abstractions
{
  public interface IEventBusFactory
  {
    IEventBus Construct();
  }
}