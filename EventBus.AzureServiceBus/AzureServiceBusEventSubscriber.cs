
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Finaps.EventBus.AzureServiceBus.Configuration;
using Finaps.EventBus.Core;
using Finaps.EventBus.Core.Abstractions;
using Finaps.EventBus.Core.Utilities;
using Microsoft.Extensions.Logging;

namespace Finaps.EventBus.AzureServiceBus
{

  internal class AzureServiceBusEventSubscriber : IEventSubscriber
  {
    private bool _disposed;
    private bool _initialized;
    private List<RuleProperties> _rules = new List<RuleProperties>();
    private ServiceBusProcessor _processor;
    private readonly ServiceBusClient _client;
    private readonly ServiceBusAdministrationClient _administrationClient;
    private readonly AzureServiceBusOptions _options;
    private readonly ILogger<AzureServiceBusEventSubscriber> _logger;

    public event AsyncEventHandler<IntegrationEventReceivedArgs> OnEventReceived;

    internal AzureServiceBusEventSubscriber(ServiceBusClient client,
      ServiceBusAdministrationClient administrationClient,
      AzureServiceBusOptions options,
      ILogger<AzureServiceBusEventSubscriber> logger)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _client = client ?? throw new ArgumentNullException(nameof(client));
      _administrationClient = administrationClient ?? throw new ArgumentNullException(nameof(administrationClient));
      _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task InitializeAsync()
    {
      if (_initialized) throw new InvalidOperationException("Subscriber has already been initialized");
      await CreateSubscription();
      await CollectExistingRules();
      _initialized = true;
    }

    private async Task CollectExistingRules()
    {
      await foreach (var rulePage in _administrationClient.GetRulesAsync(_options.TopicName, _options.SubscriptionName).AsPages())
      {
        _rules.AddRange(rulePage.Values);
      }
    }

    private async Task CreateSubscription()
    {
      await CreateSubscriptionIfNotExists();
      await RemoveDefaultRule();
    }

    private async Task CreateSubscriptionIfNotExists()
    {
      if (!await _administrationClient.SubscriptionExistsAsync(_options.TopicName, _options.SubscriptionName))
      {
        await _administrationClient.CreateSubscriptionAsync(_options.TopicName, _options.SubscriptionName);
      }
    }

    public async Task SubscribeAsync(string eventName)
    {
      if (!_initialized) throw new InvalidOperationException("Subscriber has not been initialized");
      _logger.LogInformation("Subscribing to event {EventName}", eventName);
      if (_rules.Any(r => r.Name == eventName)) return;
      if (!await _administrationClient.RuleExistsAsync(_options.TopicName, _options.SubscriptionName, eventName))
      {
        var rule = new CreateRuleOptions(eventName, new CorrelationRuleFilter()
        {
          Subject = eventName
        });
        await _administrationClient.CreateRuleAsync(_options.TopicName, _options.SubscriptionName, rule);
      }
    }

    public async Task StartConsumingAsync()
    {
      if (!_initialized) throw new InvalidOperationException("Subscriber has not been initialized");
      _processor = _client.CreateProcessor(_options.TopicName, _options.SubscriptionName, new ServiceBusProcessorOptions()
      {
        PrefetchCount = 250,
        AutoCompleteMessages = false
      });
      _processor.ProcessMessageAsync += ProcessorOnProcessMessageAsync;
      _processor.ProcessErrorAsync += ProcessorOnProcessErrorAsync;
      await _processor.StartProcessingAsync();
    }

    private async Task ProcessorOnProcessMessageAsync(ProcessMessageEventArgs arg)
    {
      var message = arg.Message;
      var eventName = message.Subject;
      var body = message.Body.ToString();
      var integrationEventReceivedArgs = new IntegrationEventReceivedArgs()
      {
        EventName = eventName,
        Message = body
      };
      await (OnEventReceived?.Invoke(this, integrationEventReceivedArgs) ?? Task.CompletedTask);
      // We do not await the Complete call because it drops our throughput considerably, and it does not need to be awaited.
      arg.CompleteMessageAsync(message).ContinueWith(t =>
      {
        if (t.IsFaulted)
        {
          _logger.LogWarning(t.Exception, $"Completing message failed, might be handled twice.\nEvent Name: {eventName}\nBody: {body}");
        }
      });
    }

    private Task ProcessorOnProcessErrorAsync(ProcessErrorEventArgs arg)
    {
      var ex = arg.Exception;
      var source = arg.ErrorSource;

      _logger.LogError(ex, $"ERROR handling message: {ex.Message} - Source: {source}");

      return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
      if (_disposed) return;
      _disposed = true;
      if (_processor != null) await _processor.DisposeAsync();
      if (_client != null) await _client.DisposeAsync();
    }

    private async Task RemoveDefaultRule()
    {
      try
      {
        if (await _administrationClient.RuleExistsAsync(_options.TopicName, _options.SubscriptionName, CreateRuleOptions.DefaultRuleName))
        {
          await _administrationClient.DeleteRuleAsync(_options.TopicName, _options.SubscriptionName,
            CreateRuleOptions.DefaultRuleName);
        }
      }
      catch (Exception ex)
      {
        _logger.LogWarning(ex, "Error while removing default rule");
      }
    }
  }
}
