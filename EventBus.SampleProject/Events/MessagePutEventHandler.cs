using System;
using System.Threading.Tasks;
using Finaps.EventBus.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace EventBus.SampleProject.Events
{
  public class MessagePutEventHandler : IIntegrationEventHandler<MessagePutEvent>
  {
    private readonly ILogger _logger;

    public MessagePutEventHandler(
      ILogger<MessagePutEventHandler> logger
    )
    {
      _logger = logger;
    }
    public async Task Handle(MessagePutEvent @event)
    {
      _logger.LogInformation($"Message put: {@event.Message}");
      await Task.CompletedTask;
    }
  }
}