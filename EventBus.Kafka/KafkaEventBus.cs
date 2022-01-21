using System;
using Finaps.EventBus.Core;
using Finaps.EventBus.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace Finaps.EventBus.Kafka
{
  internal class KafkaEventBus : DefaultEventBus
  {
    public KafkaEventBus(IEventPublisher publisher, IEventSubscriber consumer,
      IEventBusSubscriptionsManager subscriptionsManager, IServiceProvider serviceProvider, ILogger logger) : base(
      publisher, consumer, subscriptionsManager, serviceProvider, logger)
    {

    }
  }
}