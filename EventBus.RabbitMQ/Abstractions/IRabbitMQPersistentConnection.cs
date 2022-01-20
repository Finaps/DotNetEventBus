using System;
using RabbitMQ.Client;
namespace Finaps.EventBus.RabbitMq.Abstractions
{

  internal interface IRabbitMqPersistentConnection
      : IDisposable
  {
    bool IsConnected { get; }

    IModel CreateModel();

    event EventHandler<EventArgs> ConnectionRecovered;
    event EventHandler<EventArgs> ConnectionLost;
  }
}