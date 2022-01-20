using System;
using System.Threading;
using System.Threading.Tasks;
using Finaps.EventBus.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

namespace Finaps.EventBus.Core.DependencyInjection
{

  public class EventBusStartup : IHostedService
  {
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventBusStartup> _logger;

    public EventBusStartup(IServiceProvider serviceProvider, ILogger<EventBusStartup> logger)
    {
      _serviceProvider = serviceProvider;
      _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
      _logger.LogInformation($"Running {nameof(EventBusStartup)}");
      var eventBus = _serviceProvider.GetRequiredService<IEventBus>();
      await eventBus.StartConsumingAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
      return Task.CompletedTask;
    }
  }
}
