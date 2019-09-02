using System;
using System.IO;
using System.Threading.Tasks;
using Finaps.EventBus.Core.Abstractions;
using Finaps.EventBus.Core.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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

      _subscriber.OnEventReceived += Event_Received;

    }

    public void Publish(IntegrationEvent @event)
    {
      // string eventName = GetEventKey(@event);
      // var message = JsonConvert.SerializeObject(@event);
      // var body = Encoding.UTF8.GetBytes(message);
      _publisher.Publish(@event);
    }

    private string GetEventKey(IntegrationEvent @event)
    {
      return @event.GetType().Name;
    }

    private string GetEventKey<T>() where T : IntegrationEvent
    {
      return typeof(T).Name;
    }

    public void Subscribe<T, TH>()
      where T : IntegrationEvent
      where TH : IIntegrationEventHandler<T>
    {
      var eventName = GetEventKey<T>();
      SetupSubscription(eventName);
      _subscriptionsManager.AddSubscription<T, TH>();
    }

    private void SetupSubscription(string eventName)
    {
      if (!_subscriptionsManager.HasSubscriptionsForEvent(eventName))
      {
        _subscriber.Subscribe(eventName);
      }
    }

    private async void Event_Received(object sender, IntegrationEventReceivedArgs eventArgs)
    {
      string eventName = eventArgs.EventName;
      string message = eventArgs.Message;

      await ProcessEvent(eventName, message);
    }

    private async Task ProcessEvent(string eventName, string message)
    {
      _logger.LogTrace("Processing event: {EventName}", eventName);

      if (_subscriptionsManager.HasSubscriptionsForEvent(eventName))
      {
        using (var scope = _serviceProvider.CreateScope())
        {
          var handlerTypes = _subscriptionsManager.GetHandlersForEvent(eventName);
          foreach (var type in handlerTypes)
          {
            var handler = scope.ServiceProvider.GetService(type) as IIntegrationEventHandler;
            var eventType = _subscriptionsManager.GetEventTypeByName(eventName);
            var integrationEvent = JsonConvert.DeserializeObject(message, eventType);
            var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
            await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { integrationEvent });
          }
        }

      }
      else
      {
        _logger.LogWarning("No subscription for event: {EventName}", eventName);
      }
    }

    public void Dispose()
    {
      if (_disposed) return;

      _disposed = true;

      try
      {
        _publisher.Dispose();
        _subscriber.Dispose();
      }
      catch (IOException ex)
      {
        _logger.LogCritical(ex.ToString());
      }
    }
  }
}