using System;
using System.Threading.Tasks;
using Finaps.EventBus.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace EventBus.SampleProject.Events
{
  public class MessagePutEventHandler : IIntegrationEventHandler<MessagePutEvent>
  {
    private readonly ILogger _logger;
    private readonly IEventBus _eventBus;

    public MessagePutEventHandler(
      ILogger<MessagePutEventHandler> logger,
      IEventBus eventBus
    )
    {
      _logger = logger;
      _eventBus = eventBus;
    }
    public async Task Handle(MessagePutEvent @event)
    {
      _logger.LogInformation($"Message put: {@event.Message}");
      await _eventBus.PublishAsync(new MessagePostedEvent()
      {
        Message = "Posted from put"
      });
      await Task.CompletedTask;
    }
  }
}