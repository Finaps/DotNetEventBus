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
      services.AddTransient<IRabbitMQPersistentConnection>(sp =>
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
      services.AddSingleton<IEventPublisher>(sp =>
      {
        return new RabbitMQEventPublisher(
          sp.GetRequiredService<IRabbitMQPersistentConnection>(),
          options.ExchangeName,
          sp.GetRequiredService<ILogger<RabbitMQEventPublisher>>(),
          options.RetryCount
        );
      });
      services.AddSingleton<IEventSubscriber>(sp =>
      {
        return new RabbitMQEventSubscriber(
          sp.GetRequiredService<IRabbitMQPersistentConnection>(),
          options.ExchangeName,
          options.QueueName,
          sp.GetRequiredService<ILogger<RabbitMQEventSubscriber>>(),
          options.RetryCount
        );
      });
      services.AddSingleton<IEventBus>(sp =>
      {
        return new DefaultEventBus(
          sp.GetRequiredService<IEventPublisher>(),
          sp.GetRequiredService<IEventSubscriber>(),
          sp.GetRequiredService<IEventBusSubscriptionsManager>(),
          sp,
          sp.GetRequiredService<ILoggerFactory>().CreateLogger("RabbitMQ")
          );
      });
      return services;
    }


  }
}