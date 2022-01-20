using System;
using Finaps.EventBus.Core;
using Finaps.EventBus.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace Finaps.EventBus.AzureServiceBus
{

  internal class AzureServiceBusEventBus : DefaultEventBus
  {
    public AzureServiceBusEventBus(IEventPublisher publisher, IEventSubscriber subscriber,
      IEventBusSubscriptionsManager subscriptionsManager, IServiceProvider serviceProvider, ILogger logger) : base(
      publisher, subscriber, subscriptionsManager, serviceProvider, logger)
    {

    }
  }
}