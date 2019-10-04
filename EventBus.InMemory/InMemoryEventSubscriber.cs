using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Finaps.EventBus.Core;
using Finaps.EventBus.Core.Abstractions;
using Finaps.EventBus.Core.Events;
using Newtonsoft.Json;

namespace Finaps.EventBus.InMemory
{
  public class InMemoryEventSubscriber : IEventSubscriber
  {
    private readonly BlockingCollection<IntegrationEvent> _events;
    private List<string> subscriptions = new List<string>();
    public event AsyncEventHandler<IntegrationEventReceivedArgs> OnEventReceived;

    public InMemoryEventSubscriber(BlockingCollection<IntegrationEvent> events)
    {
      _events = events;
      Task.Run(async () =>
      {
        foreach (var integrationEvent in _events.GetConsumingEnumerable())
        {
          string eventName = integrationEvent.GetType().Name;
          if (subscriptions.Contains(eventName))
          {
            var eventArgs = new IntegrationEventReceivedArgs()
            {
              EventName = eventName,
              Message = JsonConvert.SerializeObject(integrationEvent)
            };
            if (OnEventReceived != null)
            {
              await OnEventReceived.Invoke(this, eventArgs);
            }

          }
        }
      });
    }

    public void Subscribe(string eventName)
    {
      subscriptions.Add(eventName);
    }

    public void Dispose()
    {
      _events.Dispose();
    }
  }
}