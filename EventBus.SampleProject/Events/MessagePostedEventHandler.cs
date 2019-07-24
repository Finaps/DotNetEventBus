using System;
using System.Threading.Tasks;
using Finaps.EventBus.Core.Abstractions;

namespace EventBus.SampleProject.Events
{
  public class MessagePostedEventHandler : IIntegrationEventHandler<MessagePostedEvent>
  {
    public async Task Handle(MessagePostedEvent @event)
    {
      Console.WriteLine(@event.Message);
      await Task.CompletedTask;
    }
  }
}