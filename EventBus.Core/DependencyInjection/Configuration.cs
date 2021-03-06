using Finaps.EventBus.Core.Abstractions;
using Finaps.EventBus.Core.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Finaps.EventBus.Core.DependencyInjection
{
  public static class Configuration
  {
    public static void AddEventHandler<T, TH>(this IApplicationBuilder app)
        where T : IntegrationEvent
        where TH : IIntegrationEventHandler<T>
    {
      var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();
      eventBus.Subscribe<T, TH>();
    }
  }
}