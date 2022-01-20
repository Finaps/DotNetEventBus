namespace Finaps.EventBus.RabbitMq.Configuration
{
  public class RabbitMqOptions
  {
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string HostName { get; set; } = "localhost";
    public int ConnectRetryCount { get; set; } = 10;
    public string ExchangeName { get; set; } = "amq.direct";
    public string QueueName { get; set; }
    public ushort PrefetchCount { get; set; } = 300;
  }
}