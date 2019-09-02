using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Finaps.EventBus.Core;
using Finaps.EventBus.Core.Abstractions;
using Finaps.EventBus.Core.Events;
using Newtonsoft.Json;

namespace Finaps.EventBus.InMemory
{
  public class InMemoryEventSubscriber : IEventSubscriber
  {
    private readonly ObservableCollection<IntegrationEvent> _events;
    private List<string> subscriptions = new List<string>();
    public event EventHandler<IntegrationEventReceivedArgs> OnEventReceived;

    public InMemoryEventSubscriber(ObservableCollection<IntegrationEvent> events)
    {
      _events = events;
      _events.CollectionChanged += Event_Collection_Changed;
    }
    public void Subscribe(string eventName)
    {
      subscriptions.Add(eventName);
    }

    private void Event_Collection_Changed(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.Action != NotifyCollectionChangedAction.Add)
      {
        return;
      }

      foreach (var integrationEvent in e.NewItems)
      {
        string eventName = integrationEvent.GetType().Name;
        if (subscriptions.Contains(eventName))
        {
          var eventArgs = new IntegrationEventReceivedArgs()
          {
            EventName = eventName,
            Message = JsonConvert.SerializeObject(integrationEvent)
          };
          OnEventReceived?.Invoke(this, eventArgs);
        }
      }
    }

  }
}