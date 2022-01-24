namespace EventBus.SampleProject.Configuration
{
  public class EventBusConfiguration
  {
    public bool UseRabbitMQ { get; set; }
    public bool UseKafka { get; set; }
    public RabbitMQConfiguration RabbitMQConfiguration { get; set; }
    public AzureServiceBusConfiguration AzureServiceBusConfiguration { get; set; }
    public KafkaConfiguration KafkaConfiguration { get; set; }
  }

  public class RabbitMQConfiguration
  {
    public string UserName { get; set; }
    public string Host { get; set; }
    public string Password { get; set; }
    public string VirtualHost { get; set; }
    public string ExchangeName { get; set; }
    public string QueueName { get; set; }
  }

  public class AzureServiceBusConfiguration
  {
    public string ConnectionString { get; set; }
    public string ClientName { get; set; }
  }

    public class KafkaConfiguration
  {
    public string Brokers { get; set; }
    public string TopicName { get; set; }
    public string GroupId { get; set; }
    public string AutoOffsetReset { get; set; }
    public string SubscriptionName { get; set; }
  }

}