using System;
using RabbitMQ.Client;

namespace Finaps.EventBus.RabbitMQ
{
  public interface IRabbitMQPersistentConnection
      : IDisposable
  {
    bool IsConnected { get; }

    bool TryConnect();

    IModel CreateModel();
  }
}