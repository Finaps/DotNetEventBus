using System;
using System.Threading.Tasks;
using Finaps.EventBus.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace EventBus.SampleProject.Events
{
  public class MessagePostedEventHandler : IIntegrationEventHandler<MessagePostedEvent>
  {
    private readonly ILogger _logger;

    public MessagePostedEventHandler(
      ILogger<MessagePostedEventHandler> logger,
      ScopedDependency dependency
    )
    {
      _logger = logger;
    }
    public async Task Handle(MessagePostedEvent @event)
    {
      _logger.LogInformation($"Message posted: {@event.Message}");
      await Task.CompletedTask;
    }
  }
}