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
      services.AddTransient<IServiceBusPersistentConnection>(sp =>
                {
                  var logger = sp.GetRequiredService<ILogger<DefaultServiceBusPersistentConnection>>();

                  var serviceBusConnection = new ServiceBusConnectionStringBuilder(options.ConnectionString);

                  return new DefaultServiceBusPersistentConnection(serviceBusConnection, logger);
                });
      services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();
      services.AddSingleton<IEventPublisher>(sp =>
      {
        return new AzureServiceBusEventPublisher(
          sp.GetRequiredService<IServiceBusPersistentConnection>(),
          sp.GetRequiredService<ILogger<AzureServiceBusEventPublisher>>()
        );
      });
      services.AddSingleton<IEventSubscriber>(sp =>
      {
        return new AzureServiceBusEventSubscriber(
          sp.GetRequiredService<IServiceBusPersistentConnection>(),
          sp.GetRequiredService<ILogger<AzureServiceBusEventSubscriber>>(),
          options.SubscriptionName
        );
      });
      services.AddSingleton<IEventBus>(sp =>
      {
        return new DefaultEventBus(
          sp.GetRequiredService<IEventPublisher>(),
          sp.GetRequiredService<IEventSubscriber>(),
          sp.GetRequiredService<IEventBusSubscriptionsManager>(),
          sp,
          sp.GetRequiredService<ILoggerFactory>().CreateLogger("AzureServiceBus")
          );
      });
      return services;
    }


  }
}