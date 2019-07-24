using Finaps.EventBus.Core.Abstractions;
using Finaps.EventBus.Core.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Finaps.EventBus.RabbitMQ.DependencyInjection
{
  public class Configuration
  {
    public void AddEventHandler<T, TH>(IApplicationBuilder app)
        where T : IntegrationEvent
        where TH : IIntegrationEventHandler<T>
    {
      var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();
      var handler = app.ApplicationServices.GetRequiredService<TH>();
      eventBus.Subscribe<T, TH>(handler);
    }
  }
}