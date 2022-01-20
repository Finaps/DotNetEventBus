
using System;
using Finaps.EventBus.Core.Models;
namespace Finaps.EventBus.Core.Utilities
{
  internal static class EventTypeUtilities
  {
    public static string GetEventKey<T>() where T : IntegrationEvent
    {
      return GetEventKey(typeof(T));
    }

    public static string GetEventKey(Type t)
    {
      return t.Name;
    }
  }
}
