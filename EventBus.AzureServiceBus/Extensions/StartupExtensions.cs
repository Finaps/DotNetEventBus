using System;
using Finaps.EventBus.AzureServiceBus.DependencyInjection;
using Finaps.EventBus.Core.Abstractions;
using Finaps.EventBus.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Finaps.EventBus.AzureServiceBus.Extensions
{
  public static class StartupExtensions
  {
    public static IServiceCollection ConfigureAzureServiceBus(this IServiceCollection services, Action<AzureServiceBusEventBusConfiguration> builder)
    {
      var config = new AzureServiceBusEventBusConfiguration();
      builder(config);

      services.AddSingleton<IEventBus>(sp =>
      {
        var loggerFactory = sp.GetService<ILoggerFactory>();
        return AzureServiceBusEventBusFactory.Create(sp, config.Subscriptions, config.Options, loggerFactory);
      });
      services.AddHostedService<EventBusStartup>();
      return services;
    }
  }
}