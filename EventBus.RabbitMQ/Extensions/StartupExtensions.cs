using System;
using Finaps.EventBus.Core.Abstractions;
using Finaps.EventBus.Core.DependencyInjection;
using Finaps.EventBus.RabbitMq.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Finaps.EventBus.RabbitMq.Extensions
{
  public static class StartupExtensions
  {
    public static IServiceCollection ConfigureRabbitMq(this IServiceCollection services, Action<RabbitMqEventBusConfiguration> builder)
    {
      var config = new RabbitMqEventBusConfiguration();
      builder(config);
      services.AddSingleton<IEventBus>(sp =>
      {
        var loggerFactory = sp.GetService<ILoggerFactory>();
        return RabbitMqEventBusFactory.Create(sp, config.Subscriptions, config.Options, loggerFactory);
      });
      services.AddHostedService<EventBusStartup>();
      return services;
    }
  }
}