using System.Collections.Generic;
using EventBus.Core.Extensions;
using EventBus.Tests.TestEvents;
using Xunit;

namespace Finaps.EventBus.Tests
{
  public class GenericTypeExtensionsTests
  {
    [Fact]
    public void GetGenericTypeNameReturnsCorrectValueForNonGenericType()
    {
      var type = typeof(TestEvent);
      Assert.Equal(type.Name, type.GetGenericTypeName());
    }

    [Fact]
    public void GetGenericTypeNameReturnsCorrectValueForGenericType()
    {
      var type = typeof(List<TestEvent>);
      Assert.Equal("List<TestEvent>", type.GetGenericTypeName());
    }
  }
}