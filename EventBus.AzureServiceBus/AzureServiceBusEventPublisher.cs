using Finaps.EventBus.Core.Abstractions;
using Finaps.EventBus.Core.Events;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Text;

namespace Finaps.EventBus.AzureServiceBus
{

  public class AzureServiceBusEventPublisher : IEventPublisher
  {
    private readonly IServiceBusPersistentConnection _serviceBusPersisterConnection;
    private readonly ILogger<AzureServiceBusEventPublisher> _logger;

    public AzureServiceBusEventPublisher(IServiceBusPersistentConnection serviceBusPersisterConnection,
        ILogger<AzureServiceBusEventPublisher> logger)
    {
      _serviceBusPersisterConnection = serviceBusPersisterConnection;
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Publish(IntegrationEvent @event)
    {
      var eventName = @event.GetType().Name;
      var jsonMessage = JsonConvert.SerializeObject(@event);
      var body = Encoding.UTF8.GetBytes(jsonMessage);

      var message = new Message
      {
        MessageId = Guid.NewGuid().ToString(),
        Body = body,
        Label = eventName,
      };

      var topicClient = _serviceBusPersisterConnection.CreateModel();

      topicClient.SendAsync(message)
          .GetAwaiter()
          .GetResult();
    }

    public void Dispose()
    {
      _serviceBusPersisterConnection.Dispose();
    }
  }
}