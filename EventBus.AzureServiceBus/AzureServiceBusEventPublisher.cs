using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Finaps.EventBus.AzureServiceBus.Configuration;
using Finaps.EventBus.Core.Abstractions;
using Microsoft.Extensions.Logging;
namespace Finaps.EventBus.AzureServiceBus
{

  internal class AzureServiceBusEventPublisher : IEventPublisher
  {
    private readonly ServiceBusSender _sender;
    private readonly ServiceBusClient _client;
    private readonly ILogger<AzureServiceBusEventPublisher> _logger;
    private bool _disposed;

    internal AzureServiceBusEventPublisher(ServiceBusClient client,
      AzureServiceBusOptions options,
      ILogger<AzureServiceBusEventPublisher> logger)
    {
      if (options == null) throw new ArgumentNullException(nameof(options));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _client = client ?? throw new ArgumentNullException(nameof(client));
      _sender = _client.CreateSender(options.TopicName);
      _logger.LogDebug($"Initialized Azure Service Bus Sender with topic name {options.TopicName}");
    }

    public async Task PublishAsync(string body, string eventName, string messageId)
    {
      _logger.LogTrace($"Publishing message to Azure Service Bus.\nBody: {body}\nEvent Name: {eventName}\nMessage Id: {messageId}");
      if (body == null) throw new ArgumentNullException(nameof(body));
      if (eventName == null) throw new ArgumentNullException(nameof(eventName));
      if (messageId == null) throw new ArgumentNullException(nameof(messageId));

      var message = new ServiceBusMessage(body)
      {
        MessageId = messageId,
        Subject = eventName
      };

      // Do not await call to increase throughput considerably
      _sender.SendMessageAsync(message).ContinueWith(t =>
      {
        if (t.IsFaulted)
        {
          _logger.LogError(t.Exception, $"Sending message to Azure Service bus failed.\nEvent Type: {eventName}\nReason: {t.Exception.Message}");
        }
      });
    }

    public async ValueTask DisposeAsync()
    {
      if (_disposed) return;
      _disposed = true;
      await _sender.DisposeAsync();
      await _client.DisposeAsync();
    }
  }
}