using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Finaps.EventBus.Core.Abstractions;
using Finaps.EventBus.IntegrationTests.Events;
using Finaps.EventBus.RabbitMQ;
using Finaps.EventBus.RabbitMQ.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Finaps.EventBus.IntegrationTests
{
  public class RabbitMQEventBusTests : BaseEventBusTests
  {

    public RabbitMQEventBusTests() : base(EventBusType.RabbitMQ) { }


  }
}
