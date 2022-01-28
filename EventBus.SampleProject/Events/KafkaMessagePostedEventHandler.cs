using System;
using System.Threading.Tasks;
using Finaps.EventBus.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace EventBus.SampleProject.Events
{
  public class KafkaMessagePostedEventHandler : IIntegrationEventHandler<KafkaMessagePostedEvent>
  {
    private readonly ILogger _logger;

    public KafkaMessagePostedEventHandler(
      ILogger<KafkaMessagePostedEventHandler> logger,
      ScopedDependency dependency
    )
    {
      _logger = logger;
    }
    public async Task Handle(KafkaMessagePostedEvent @event)
    {
      _logger.LogInformation($"Kafka message posted: {@event.Message}");
      await Task.CompletedTask;
    }
  }
}