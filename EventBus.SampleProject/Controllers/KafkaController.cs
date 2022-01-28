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
    public async Task<ObjectResult> Post([FromBody] KafkaMessageModel value)
    {

      await _eventBus.PublishAsync(new KafkaMessagePostedEvent()
      {
        Message = value.Message,
        Topic = value.Topic
      });
      return Ok("sent");
    }

    public class KafkaMessageModel
    {
      public string Message { get; set; }
      public string Topic { get; set; }
    }

  }
}
