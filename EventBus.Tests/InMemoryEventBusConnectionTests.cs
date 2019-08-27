using EventBus.Core;
using Finaps.EventBus.Core;
using Xunit;

namespace EventBus.Tests
{
  public class InMemoryEventBusConnectionTests
  {
    [Fact]
    public void CorrectEventIsRaised()
    {
      string eventName = "event";
      string message = "message";
      string eventNameReceived = "";
      string messageReceived = "";

      var connection = new InMemoryEventBusConnection();
      connection.OnEventReceived += (object sender, IntegrationEventReceivedArgs eventArgs) =>
      {
        eventNameReceived = eventArgs.EventName;
        messageReceived = eventArgs.Message;
      };
      connection.Subscribe(eventName);
      connection.Publish(eventName, message);
      Assert.Equal(eventName, eventNameReceived);
      Assert.Equal(message, messageReceived);
    }

    [Fact]
    public void NoEventIsRaisedWhenNotSubscribed()
    {
      bool eventRaised = false;
      var connection = new InMemoryEventBusConnection();
      connection.OnEventReceived += (object sender, IntegrationEventReceivedArgs eventReceivedArgs) =>
      {
        eventRaised = true;
      };
      connection.Publish("event", "message");
      Assert.False(eventRaised);
    }
  }
}