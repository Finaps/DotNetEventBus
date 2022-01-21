using System;
using Finaps.EventBus.Kafka.DependencyInjection;
using Finaps.EventBus.Core.Abstractions;
using Finaps.EventBus.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Finaps.EventBus.Kafka.Extensions
{
  public static class StartupExtensions
  {
    public static IServiceCollection ConfigureKafkaServiceBus(this IServiceCollection services, Action<KafkaServiceBusEventBusConfiguration> builder)
    {
      var config = new KafkaServiceBusEventBusConfiguration();
      builder(config);

      services.AddSingleton<IEventBus>(sp =>
      {
        var loggerFactory = sp.GetService<ILoggerFactory>();
        return KafkaEventBusFactory.Create(sp, config.Subscriptions, config.Options, loggerFactory);
      });
      services.AddHostedService<EventBusStartup>();
      return services;
    }
  }
}