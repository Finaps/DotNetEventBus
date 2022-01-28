using System;

namespace Finaps.EventBus.Core.Models
{

  public class IntegrationEvent
  {
    public IntegrationEvent()
    {
      Id = Guid.NewGuid();
      CreationDate = DateTime.UtcNow;
    }
    public Guid Id { get; set; }
    public DateTime CreationDate { get; set; }
    public string? UserId { get; set; }
  }
}