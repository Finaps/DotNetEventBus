using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Finaps.EventBus.Core.Abstractions;
using Finaps.EventBus.IntegrationTests.Events;
using Finaps.EventBus.RabbitMQ;
using Finaps.EventBus.RabbitMQ.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Finaps.EventBus.IntegrationTests
{
  public class RabbitMQEventBusTests : BaseEventBusTests
  {

    private static readonly int ConsumeTimeoutInMilliSeconds = 5000;

    public RabbitMQEventBusTests() : base(EventBusType.RabbitMQ) { }

    // [Fact]
    // public void ListensCorrectly()
    // {
    //   var subscriptionTestEvent = PublishSubscriptionTestEvent();
    //   var eventReceived = autoResetEvent.WaitOne(ConsumeTimeoutInMilliSeconds);
    //   Assert.True(eventReceived);
    //   var consumedEvent = eventReceivedNotifier.Events.Single() as SubscriptionTestEvent;
    //   Assert.Equal(subscriptionTestEvent.TestString, consumedEvent.TestString);
    //   Assert.Equal(subscriptionTestEvent.Id, consumedEvent.Id);
    //   Assert.Equal(subscriptionTestEvent.CreationDate, consumedEvent.CreationDate);

    // }

    // [Fact]
    // public void CanPublishWhileReceiving()
    // {
    //   var eventPublisherEvent = new EventPublisherEvent();
    //   eventBus.Publish(eventPublisherEvent);
    //   var eventReceived = autoResetEvent.WaitOne(ConsumeTimeoutInMilliSeconds);
    //   Assert.True(eventReceived);
    //   Assert.NotEmpty(eventReceivedNotifier.Events);
    // }

    // [Fact]
    // public void EventsAreReceivedInOrder()
    // {
    //   var publishedEvents = new List<SubscriptionTestEvent>();
    //   for (int i = 0; i < 50; i++)
    //   {
    //     publishedEvents.Add(PublishSubscriptionTestEvent());
    //   }
    //   var eventReceived = autoResetEvent.WaitOne(ConsumeTimeoutInMilliSeconds);
    //   Assert.True(eventReceived);
    //   var publishedGuids = publishedEvents.Select(@event => @event.Id);
    //   var consumedGuids = eventReceivedNotifier.Events.Select(@event => @event.Id);
    //   Assert.Equal(publishedGuids, consumedGuids);
    // }


  }
}
