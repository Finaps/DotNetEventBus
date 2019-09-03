namespace Finaps.EventBus.AzureServiceBus
{
  using Microsoft.Azure.ServiceBus;
  using System;

  public interface IServiceBusPersistentConnection : IDisposable
  {
    ServiceBusConnectionStringBuilder ServiceBusConnectionStringBuilder { get; }

    ITopicClient CreateModel();
  }
}