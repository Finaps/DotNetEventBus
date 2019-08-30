using System;
using System.Linq;
using EventBus.Tests.TestEvents;
using Finaps.EventBus.Core;
using Xunit;

namespace EventBus.Tests
{
  public class InMemoryEventBusSubscriptionsManagerTests
  {
    [Fact]
    public void IsEmptyReturnsTrueWhenEmpty()
    {
      var manager = new InMemoryEventBusSubscriptionsManager();
      Assert.True(manager.IsEmpty);
    }

    [Fact]
    public void IsEmptyReturnsFalseWhenNotEmpty()
    {
      var manager = new InMemoryEventBusSubscriptionsManager();
      manager.AddSubscription<TestEvent, TestEventHandler>();
      Assert.False(manager.IsEmpty);
    }

    [Fact]
    public void IsEmptyAfterClear()
    {
      var manager = new InMemoryEventBusSubscriptionsManager();
      manager.AddSubscription<TestEvent, TestEventHandler>();
      manager.Clear();
      Assert.True(manager.IsEmpty);
    }

    [Fact]
    public void ThrowsIfHandlerIsAlreadyRegistered()
    {
      var manager = new InMemoryEventBusSubscriptionsManager();
      manager.AddSubscription<TestEvent, TestEventHandler>();
      Assert.Throws<ArgumentException>(() => manager.AddSubscription<TestEvent, TestEventHandler>());
    }

    [Fact]
    public void CanGetHandlersForEventByType()
    {
      var manager = new InMemoryEventBusSubscriptionsManager();
      manager.AddSubscription<TestEvent, TestEventHandler>();
      var handlerTypes = manager.GetHandlersForEvent<TestEvent>();
      Assert.Equal(typeof(TestEventHandler), handlerTypes.First());
    }

    [Fact]
    public void CanGetHandlersForEventByName()
    {
      var manager = new InMemoryEventBusSubscriptionsManager();
      manager.AddSubscription<TestEvent, TestEventHandler>();
      var handlerTypes = manager.GetHandlersForEvent("TestEvent");
      Assert.Equal(typeof(TestEventHandler), handlerTypes.First());
    }

    [Fact]
    public void HasSubscriptionsForEventReturnsTrueIfItHasSubscription()
    {
      var manager = new InMemoryEventBusSubscriptionsManager();
      manager.AddSubscription<TestEvent, TestEventHandler>();
      Assert.True(manager.HasSubscriptionsForEvent<TestEvent>());
    }

    [Fact]
    public void HasSubscriptionsForEventByNameReturnsTrueIfItHasSubscription()
    {
      var manager = new InMemoryEventBusSubscriptionsManager();
      manager.AddSubscription<TestEvent, TestEventHandler>();
      Assert.True(manager.HasSubscriptionsForEvent("TestEvent"));
    }

    [Fact]
    public void HasSubscriptionsForEventReturnsFalseIfItHasNoSubscription()
    {
      var manager = new InMemoryEventBusSubscriptionsManager();
      Assert.False(manager.HasSubscriptionsForEvent<TestEvent>());
    }

    [Fact]
    public void GetEventTypeByNameReturnsCorrectType()
    {
      var manager = new InMemoryEventBusSubscriptionsManager();
      manager.AddSubscription<TestEvent, TestEventHandler>();
      var eventType = manager.GetEventTypeByName("TestEvent");
      Assert.Equal(typeof(TestEvent), eventType);
    }

    [Fact]
    public void GetEventKeyReturnsCorrectValue()
    {
      var manager = new InMemoryEventBusSubscriptionsManager();
      string key = manager.GetEventKey<TestEvent>();
      Assert.Equal("TestEvent", key);
    }
  }
}