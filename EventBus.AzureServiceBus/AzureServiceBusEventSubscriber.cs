using Finaps.EventBus.Core;
using Finaps.EventBus.Core.Abstractions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finaps.EventBus.AzureServiceBus
{
  public class AzureServiceBusEventSubscriber : IEventSubscriber
  {
    private readonly IServiceBusPersistentConnection _serviceBusPersisterConnection;
    private readonly ILogger<AzureServiceBusEventSubscriber> _logger;
    private readonly SubscriptionClient _subscriptionClient;
    public event AsyncEventHandler<IntegrationEventReceivedArgs> OnEventReceived;

    public AzureServiceBusEventSubscriber(IServiceBusPersistentConnection serviceBusPersisterConnection,
        ILogger<AzureServiceBusEventSubscriber> logger, string subscriptionClientName)
    {
      _serviceBusPersisterConnection = serviceBusPersisterConnection;
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));

      _subscriptionClient = new SubscriptionClient(serviceBusPersisterConnection.ServiceBusConnectionStringBuilder,
          subscriptionClientName);

      RemoveDefaultRule();
      RegisterSubscriptionClientMessageHandler();
    }

    public void Subscribe(string eventName)
    {
      try
      {
        if (_subscriptionClient.GetRulesAsync().GetAwaiter().GetResult().Any(rule => rule.Name == eventName))
        {
          _logger.LogWarning("The messaging entity {eventName} already exists.", eventName);
          return;
        }

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

      _logger.LogInformation("Subscribing to event {EventName}", eventName);
    }

    public void Dispose()
    {
      _serviceBusPersisterConnection.Dispose();
      _subscriptionClient.CloseAsync();
    }

    private void RegisterSubscriptionClientMessageHandler()
    {
      _subscriptionClient.RegisterMessageHandler(
        async (message, token) =>
        {
          var eventName = $"{message.Label}";
          var messageData = Encoding.UTF8.GetString(message.Body);
          var integrationEventReceivedArgs = new IntegrationEventReceivedArgs()
          {
            EventName = eventName,
            Message = messageData
          };
          await Task.Run(async () => await (OnEventReceived?.Invoke(this, integrationEventReceivedArgs) ?? Task.CompletedTask));
          await _subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
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