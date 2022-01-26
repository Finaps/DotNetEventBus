using System.Threading.Tasks;
using EventBus.SampleProject.Events;
using Finaps.EventBus.Core.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace EventBus.SampleProject.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class KafkaController : ControllerBase
  {
    private readonly IEventBus _eventBus;
    public KafkaController(IEventBus eventBus)
    {
      _eventBus = eventBus;
    }

    [HttpPost]
    public async Task<ObjectResult> Post([FromBody] MessageModel value)
    {

      await _eventBus.PublishAsync(new MessagePostedEvent()
      {
        Message = value.Message
      });
      return Ok("sent");
    }

    [HttpPut]
    public async Task<ObjectResult> Put([FromBody] MessageModel value)
    {
      await _eventBus.PublishAsync(new MessagePutEvent()
      {
        Message = value.Message
      });

      return Ok("sent");
    }

    public class MessageModel
    {
      public string Message { get; set; }
    }

  }
}
