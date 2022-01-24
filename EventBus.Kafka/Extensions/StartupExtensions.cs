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
    public static IServiceCollection ConfigureKafka(this IServiceCollection services, Action<KafkaConfiguration> builder)
    {
      var config = new KafkaConfiguration();
      builder(config);

      services.AddSingleton<IEventBus>(sp =>
      {
        var loggerFactory = sp.GetService<ILoggerFactory>();
        return KafkaFactory.Create(sp, config.Subscriptions, config.Options, loggerFactory);
      });
      services.AddHostedService<EventBusStartup>();
      return services;
    }
  }
}