using Finaps.EventBus.Core;
using Finaps.EventBus.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Finaps.EventBus.RabbitMQ.DependencyInjection
{
  public static class DependencyInjectionExtensions
  {
    public static IServiceCollection AddRabbitMQ(this IServiceCollection services, RabbitMQOptions options = null)
    {
      options = options ?? new RabbitMQOptions();
      services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
                {
                  var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();

                  var factory = new ConnectionFactory()
                  {
                    HostName = options.HostName,
                    DispatchConsumersAsync = true,
                    UserName = options.UserName,
                    Password = options.Password,
                  };

                  return new DefaultRabbitMQPersistentConnection(factory, logger, options.RetryCount);
                });
      services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();
      services.AddSingleton<IEventBus>(sp =>
      {
        return new EventBusRabbitMQ(
          sp.GetRequiredService<IRabbitMQPersistentConnection>(),
          sp.GetRequiredService<ILogger<EventBusRabbitMQ>>(),
          sp.GetRequiredService<IEventBusSubscriptionsManager>(),
          sp,
          options.ExchangeName,
          options.QueueName,
          options.RetryCount
          );
      });
      return services;
    }


  }
}