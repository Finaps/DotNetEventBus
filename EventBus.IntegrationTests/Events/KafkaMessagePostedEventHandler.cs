using Finaps.EventBus.Core.Abstractions;

namespace Finaps.EventBus.IntegrationTests.Events;
public class KafkaTestEventHandler : IIntegrationEventHandler<KafkaTestEvent>
{
  private readonly EventReceivedNotifier _eventReceivedNotifier;

  public KafkaTestEventHandler(EventReceivedNotifier eventReceivedNotifier)
  {
    _eventReceivedNotifier = eventReceivedNotifier;
  }
  public Task Handle(KafkaTestEvent @event)
  {
    _eventReceivedNotifier.NotifyEventReceived(@event);
    return Task.CompletedTask;
  }
}