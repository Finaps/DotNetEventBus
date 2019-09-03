using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using System;

namespace Finaps.EventBus.AzureServiceBus
{
  public class DefaultServiceBusPersistentConnection : IServiceBusPersistentConnection
  {
    private readonly ILogger<DefaultServiceBusPersistentConnection> _logger;
    private readonly ServiceBusConnectionStringBuilder _serviceBusConnectionStringBuilder;
    private ITopicClient _topicClient;

    bool _disposed;

    public DefaultServiceBusPersistentConnection(ServiceBusConnectionStringBuilder serviceBusConnectionStringBuilder,
        ILogger<DefaultServiceBusPersistentConnection> logger)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));

      _serviceBusConnectionStringBuilder = serviceBusConnectionStringBuilder ??
          throw new ArgumentNullException(nameof(serviceBusConnectionStringBuilder));
      _topicClient = new TopicClient(_serviceBusConnectionStringBuilder, RetryPolicy.Default);
    }

    public ServiceBusConnectionStringBuilder ServiceBusConnectionStringBuilder => _serviceBusConnectionStringBuilder;

    public ITopicClient CreateModel()
    {
      if (_topicClient.IsClosedOrClosing)
      {
        _topicClient = new TopicClient(_serviceBusConnectionStringBuilder, RetryPolicy.Default);
      }

      return _topicClient;
    }

    public void Dispose()
    {
      if (_disposed) return;

      _disposed = true;
      _topicClient.CloseAsync();
    }
  }
}
