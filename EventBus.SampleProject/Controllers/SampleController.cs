using System.Threading.Tasks;
using EventBus.SampleProject.Events;
using Finaps.EventBus.Core.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace EventBus.SampleProject.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class SampleController : ControllerBase
  {
    private readonly IEventBus _eventBus;
    public SampleController(IEventBus eventBus)
    {
      _eventBus = eventBus;
    }

    [HttpPost("rabbit")]
    public async Task<ObjectResult> Post([FromBody] MessageModel value)
    {

      await _eventBus.PublishAsync(new MessagePostedEvent()
      {
        Message = value.Message
      });
      return Ok("sent");
    }

    [HttpPut("rabbit")]
    public async Task<ObjectResult> Put([FromBody] MessageModel value)
    {
      await _eventBus.PublishAsync(new MessagePutEvent()
      {
        Message = value.Message
      });

      return Ok("sent");
    }

    [HttpPost("kafka")]
    public async Task<ObjectResult> Post([FromBody] KafkaMessageModel value)
    {

      await _eventBus.PublishAsync(new KafkaMessagePostedEvent()
      {
        Message = value.Message,
        Topic = value.Topic
      });
      return Ok("sent");
    }

    public class MessageModel
    {
      public string Message { get; set; }
    }

    public class KafkaMessageModel
    {
      public string Message { get; set; }
      public string Topic { get; set; }
    }

  }
}
