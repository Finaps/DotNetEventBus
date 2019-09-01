using System;
using Finaps.EventBus.RabbitMQ;
using Xunit;

namespace Finaps.EventBus.IntegrationTests
{
  public class RabbitTests
  {
    [Fact]
    public void CanCreateConnection()
    {
      var factory = new RabbitMQEventBusFactory();
      var eventBus = factory.Construct();
    }
  }
}
