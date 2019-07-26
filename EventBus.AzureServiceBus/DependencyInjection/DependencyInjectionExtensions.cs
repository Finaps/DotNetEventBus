using Finaps.EventBus.Core;
using Finaps.EventBus.Core.Abstractions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Finaps.EventBus.AzureServiceBus.DependencyInjection
{
  public static class DependencyInjectionExtensions
  {
    public static IServiceCollection AddAzureServiceBus(this IServiceCollection services, AzureServiceBusOptions options = null)
    {
      options = options ?? new AzureServiceBusOptions();
      services.AddSingleton<IServiceBusPersisterConnection>(sp =>
                {
                  var logger = sp.GetRequiredService<ILogger<DefaultServiceBusPersisterConnection>>();

                  var serviceBusConnection = new ServiceBusConnectionStringBuilder(options.ConnectionString);

                  return new DefaultServiceBusPersisterConnection(serviceBusConnection, logger);
                });
      services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();
      services.AddSingleton<IEventBus>(sp =>
      {
        return new EventBusServiceBus(
          sp.GetRequiredService<IServiceBusPersisterConnection>(),
          sp.GetRequiredService<ILogger<EventBusServiceBus>>(),
          sp.GetRequiredService<IEventBusSubscriptionsManager>(),
          sp,
          options.ClientName
          );
      });
      return services;
    }


  }
}