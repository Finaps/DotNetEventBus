namespace Finaps.EventBus.AzureServiceBus
{
  using Finaps.EventBus.Core;
  using Finaps.EventBus.Core.Abstractions;
  using Finaps.EventBus.Core.Events;
  using Microsoft.Azure.ServiceBus;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Logging;
  using Newtonsoft.Json;
  using System;
  using System.Text;
  using System.Threading.Tasks;

  public class EventBusServiceBus : IEventBus
  {
    private readonly IServiceBusPersisterConnection _serviceBusPersisterConnection;
    private readonly ILogger<EventBusServiceBus> _logger;
    private readonly IEventBusSubscriptionsManager _subsManager;
    private readonly SubscriptionClient _subscriptionClient;
    private readonly IServiceProvider _serviceProvider;

    public EventBusServiceBus(IServiceBusPersisterConnection serviceBusPersisterConnection,
        ILogger<EventBusServiceBus> logger, IEventBusSubscriptionsManager subsManager, IServiceProvider serviceProvider, string subscriptionClientName)
    {
      _serviceBusPersisterConnection = serviceBusPersisterConnection;
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _subsManager = subsManager ?? new InMemoryEventBusSubscriptionsManager();

      _subscriptionClient = new SubscriptionClient(serviceBusPersisterConnection.ServiceBusConnectionStringBuilder,
          subscriptionClientName);

      _serviceProvider = serviceProvider;

      RemoveDefaultRule();
      RegisterSubscriptionClientMessageHandler();
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

    public void Subscribe<T, TH>()
        where T : IntegrationEvent
        where TH : IIntegrationEventHandler<T>
    {
      var eventName = typeof(T).Name;

      var containsKey = _subsManager.HasSubscriptionsForEvent<T>();
      if (!containsKey)
      {
        try
        {
          _subscriptionClient.AddRuleAsync(new RuleDescription
          {
            Filter = new CorrelationFilter { Label = eventName },
            Name = eventName
          }).GetAwaiter().GetResult();
        }
        catch (ServiceBusException)
        {
          _logger.LogWarning("The messaging entity {eventName} already exists.", eventName);
        }
      }

      _logger.LogInformation("Subscribing to event {EventName} with {EventHandler}", eventName, typeof(TH).Name);

      _subsManager.AddSubscription<T, TH>();
    }

    public void Dispose()
    {
      _subsManager.Clear();
    }

    private void RegisterSubscriptionClientMessageHandler()
    {
      _subscriptionClient.RegisterMessageHandler(
          async (message, token) =>
          {
            var eventName = $"{message.Label}";
            var messageData = Encoding.UTF8.GetString(message.Body);

            // Complete the message so that it is not received again.
            if (await ProcessEvent(eventName, messageData))
            {
              await _subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
            }
          },
          new MessageHandlerOptions(ExceptionReceivedHandler) { MaxConcurrentCalls = 10, AutoComplete = false });
    }

    private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
    {
      var ex = exceptionReceivedEventArgs.Exception;
      var context = exceptionReceivedEventArgs.ExceptionReceivedContext;

      _logger.LogError(ex, "ERROR handling message: {ExceptionMessage} - Context: {@ExceptionContext}", ex.Message, context);

      return Task.CompletedTask;
    }

    private async Task<bool> ProcessEvent(string eventName, string message)
    {
      var processed = false;
      if (_subsManager.HasSubscriptionsForEvent(eventName))
      {
        var handlerTypes = _subsManager.GetHandlersForEvent(eventName);
        using (var scope = _serviceProvider.CreateScope())
        {
          foreach (var type in handlerTypes)
          {
            var handler = scope.ServiceProvider.GetService(type) as IIntegrationEventHandler;
            var eventType = _subsManager.GetEventTypeByName(eventName);
            var integrationEvent = JsonConvert.DeserializeObject(message, eventType);
            var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
            await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { integrationEvent });
          }
          processed = true;
        }
      }
      return processed;
    }

    private void RemoveDefaultRule()
    {
      try
      {
        _subscriptionClient
         .RemoveRuleAsync(RuleDescription.DefaultRuleName)
         .GetAwaiter()
         .GetResult();
      }
      catch (MessagingEntityNotFoundException)
      {
        _logger.LogWarning("The messaging entity {DefaultRuleName} Could not be found.", RuleDescription.DefaultRuleName);
      }
    }
  }
}