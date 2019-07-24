namespace Finaps.EventBus.RabbitMQ
{
  public class RabbitMQOptions
  {
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string HostName { get; set; } = "localhost";
    public int RetryCount { get; set; } = 5;
    public string ExchangeName { get; set; } = "amq.direct";
    public string QueueName { get; set; }
  }
}