using System.Threading.Tasks;
using Finaps.EventBus.Core.Abstractions;

namespace Finaps.EventBus.IntegrationTests.Events
{
  public class SubscriptionTestEventHandler : IIntegrationEventHandler<SubscriptionTestEvent>
  {
    private readonly EventReceivedNotifier _eventReceivedNotifier;

    public SubscriptionTestEventHandler(EventReceivedNotifier eventReceivedNotifier)
    {
      _eventReceivedNotifier = eventReceivedNotifier;
    }
    public Task Handle(SubscriptionTestEvent @event)
    {
      _eventReceivedNotifier.NotifyEventReceived(@event);
      return Task.CompletedTask;
    }
  }
}