using EventBus.SampleProject.Events;
using Finaps.EventBus.Core.DependencyInjection;
using Finaps.EventBus.RabbitMq.Configuration;
using Finaps.EventBus.Kafka.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Finaps.EventBus.AzureServiceBus.Extensions;
using Finaps.EventBus.RabbitMq.Extensions;
using Finaps.EventBus.Kafka.Extensions;

namespace EventBus.SampleProject.Infrastructure.EventBus
{
  public static class EventBusStartup
  {
    public static IServiceCollection ConfigureAzureServiceBus(this IServiceCollection services,
      IConfiguration configuration)
    {
      var azureConfig = configuration.GetSection(AzureServiceBusConfiguration.ConfigurationKey).Get<AzureServiceBusConfiguration>();

      services.ConfigureAzureServiceBus(config =>
      {
        config.Options.ConnectionString = azureConfig.ConnectionString;
        config.Options.SubscriptionName = azureConfig.SubscriptionName;
        config.Options.TopicName = azureConfig.TopicName;
        SetupSubscriptions(config);
      });
      return services;
    }

    private class AzureServiceBusConfiguration
    {
      public const string ConfigurationKey = "AzureServiceBus";

      public string ConnectionString { get; set; }
      public string TopicName { get; set; }
      public string SubscriptionName { get; set; }
    }

    public static IServiceCollection ConfigureRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
      var rabbitConfig = new RabbitMqOptions();
      configuration.GetSection("Rabbit").Bind(rabbitConfig);

      services.ConfigureRabbitMq(config =>
      {
        config.Options = rabbitConfig;
        SetupSubscriptions(config);
      });
      return services;
    }

    private static void SetupSubscriptions(BaseEventBusConfiguration config)
    {
      config.AddSubscription<MessagePostedEvent, MessagePostedEventHandler>();
      config.AddSubscription<MessagePutEvent, MessagePutEventHandler>();
      config.AddSubscription<KafkaMessagePostedEvent, KafkaMessagePostedEventHandler>();
    }

    public static IServiceCollection ConfigureKafka(this IServiceCollection services, IConfiguration configuration)
    {
      var kafkaConfig = new KafkaOptions();
      configuration.GetSection("Kafka").Bind(kafkaConfig);

      services.ConfigureKafka(config =>
      {
        config.Options = kafkaConfig;
        SetupSubscriptions(config);
      });
      return services;
    }
  }
}
