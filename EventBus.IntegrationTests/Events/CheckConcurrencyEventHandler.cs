using System.Threading.Tasks;
using Finaps.EventBus.Core.Abstractions;
using System;

namespace EventBus.IntegrationTests.Events
{
  public class CheckConcurrencyEventHandler : IIntegrationEventHandler<CheckConcurrencyEvent>
  {
    private readonly IntegerIncrementer _integerIncrementer;

    public CheckConcurrencyEventHandler(IntegerIncrementer integerIncrementer)
    {
      _integerIncrementer = integerIncrementer;
    }
    public async Task Handle(CheckConcurrencyEvent @event)
    {
      await _integerIncrementer.Increment();
    }
  }
}