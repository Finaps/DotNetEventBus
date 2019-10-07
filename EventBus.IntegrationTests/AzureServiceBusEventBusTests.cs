using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Finaps.EventBus.IntegrationTests.Events;
using System.Linq;
using Finaps.EventBus.Core.Abstractions;
using Finaps.EventBus.InMemory.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Finaps.EventBus.IntegrationTests
{
  public class AzureServiceBusEventBusTests : BaseEventBusTests
  {
    public AzureServiceBusEventBusTests() : base(EventBusType.Azure) { }


  }
}