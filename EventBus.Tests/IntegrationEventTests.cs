using System;
using EventBus.Tests.TestEvents;
using Xunit;

namespace EventBus.Tests
{
  public class IntegrationEventTests
  {
    [Fact]
    public void IdIsGenerated()
    {
      var integrationEvent = new TestEvent();
      Assert.NotEqual(Guid.Empty, integrationEvent.Id);
    }

    [Fact]
    public void CreatedDateIsGenerated()
    {
      var integrationEvent = new TestEvent();
      Assert.Equal(DateTime.UtcNow, integrationEvent.CreationDate, TimeSpan.FromSeconds(1));
    }
  }
}
