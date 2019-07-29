using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventBus.SampleProject.Events;
using Finaps.EventBus.Core.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace EventBus.SampleProject.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class RabbitController : ControllerBase
  {
    private readonly IEventBus _eventBus;
    public RabbitController(IEventBus eventBus)
    {
      _eventBus = eventBus;
    }

    [HttpPost]
    public void Post([FromBody] string value)
    {
      _eventBus.Publish(new MessagePostedEvent()
      {
        Message = value
      });
    }

    [HttpPut]
    public void Put([FromBody] string value)
    {
      _eventBus.Publish(new MessagePutEvent()
      {
        Message = value
      });
    }

  }
}
