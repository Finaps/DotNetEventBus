namespace Finaps.EventBus.IntegrationTests
{
  public class InMemoryEventBusTests : BaseEventBusTests
  {
    public InMemoryEventBusTests() : base(EventBusType.In_Memory) { }

  }
}