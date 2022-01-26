using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Finaps.EventBus.Core.Abstractions;
using Finaps.EventBus.Core.Models;
using Finaps.EventBus.Core.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Finaps.EventBus.Core
{
  public class DefaultEventBus : IEventBus
  {
    private readonly IEventPublisher _publisher;
    private readonly IEventSubscriber _subscriber;
    private readonly IEventBusSubscriptionsManager _subscriptionsManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;
    private bool _disposed;

    public DefaultEventBus(
      IEventPublisher publisher,
      IEventSubscriber subscriber,
      IEventBusSubscriptionsManager subscriptionsManager,
      IServiceProvider serviceProvider,
      ILogger logger
    )
    {
      _subscriber = subscriber;
      _publisher = publisher;
      _subscriptionsManager = subscriptionsManager;
      _serviceProvider = serviceProvider;
      _logger = logger;

      _subscriber.OnEventReceived += OnEventReceived;

    }

    public async Task PublishAsync(IntegrationEvent @event)
    {
      string messageId = @event.Id.ToString();
      string message = JsonSerializer.Serialize(@event, @event.GetType());
      string eventName = GetEventKey(@event);
      await _publisher.PublishAsync(message, eventName, messageId);
    }

    private string GetEventKey(IntegrationEvent @event)
    {
      return EventTypeUtilities.GetEventKey(@event.GetType());
    }

    public void AddSubscription(EventSubscription subscription)
    {
      var eventName = EventTypeUtilities.GetEventKey(subscription.EventType);
      _logger.LogInformation($"Add subscription for events of type {eventName}");
      _subscriptionsManager.AddSubscription(subscription);
    }

    public async Task StartConsumingAsync()
    {
      await _subscriber.InitializeAsync();
      _logger.LogInformation($"Subscribing to events");
      foreach (var eventName in _subscriptionsManager.GetSubscriptions())
      {
        await _subscriber.SubscribeAsync(eventName);
      }
      _logger.LogInformation("Starting consumption of integration events");
      await _subscriber.StartConsumingAsync();
    }


    private async Task OnEventReceived(object sender, IntegrationEventReceivedArgs eventArgs)
    {
      string eventName = eventArgs.EventName ??= "";
      string message = eventArgs.Message ??= "";

      await ProcessEvent(eventName, message);
    }

    private async Task ProcessEvent(string eventName, string message)
    {
      _logger.LogTrace("Processing event: {EventName}", eventName);

      if (_subscriptionsManager.HasSubscriptionsForEvent(eventName))
      {
        using var scope = _serviceProvider.CreateScope();
        var handlerTypes = _subscriptionsManager.GetHandlersForEvent(eventName);
        foreach (var type in handlerTypes)
        {
          var handler = scope.ServiceProvider.GetService(type) as IIntegrationEventHandler;
          var eventType = _subscriptionsManager.GetEventTypeByName(eventName);
          var integrationEvent = JsonSerializer.Deserialize(message, eventType);
          var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
          await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { integrationEvent });
        }
      }
      else
      {
        _logger.LogWarning("No subscription for event: {EventName}", eventName);
      }
    }

    public async ValueTask DisposeAsync()
    {
      if (_disposed) return;
      _logger.LogTrace($"Disposing {this.GetType().Name}");
      _disposed = true;

      _subscriber.OnEventReceived -= OnEventReceived;

      try
      {
        await _publisher.DisposeAsync();
        await _subscriber.DisposeAsync();
      }
      catch (IOException ex)
      {
        _logger.LogCritical(ex.ToString());
      }
      _logger.LogTrace($"{this.GetType().Name} disposed");
    }
  }
}
