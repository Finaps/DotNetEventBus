namespace EventBus.SampleProject.Configuration
{
  public class EventBusConfiguration
  {
    public bool UseRabbitMQ { get; set; }
    public RabbitMQConfiguration RabbitMQConfiguration { get; set; }
    public AzureServiceBusConfiguration AzureServiceBusConfiguration { get; set; }
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

}