using System.Threading.Tasks;

namespace Finaps.EventBus.Core.Utilities
{
  public delegate Task AsyncEventHandler<T>(object sender, T eventReceivedArgs);
}