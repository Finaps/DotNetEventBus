using System.Threading.Tasks;
using Finaps.EventBus.Core.Abstractions;

namespace EventBus.Tests.TestEvents
{
  public class TestEventHandler : IIntegrationEventHandler<TestEvent>
  {
    public async Task Handle(TestEvent @event)
    {
      await Task.CompletedTask;
    }
  }
}