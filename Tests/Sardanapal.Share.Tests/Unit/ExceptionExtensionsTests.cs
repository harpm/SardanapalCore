using FluentAssertions;
using Sardanapal.Share.Extensions;
using Xunit;

namespace Sardanapal.Share.Tests.Unit;

public class ExceptionExtensionsTests
{
    [Fact]
    public void GetHierarchicalMessages_ShouldReturnEmpty_WhenExceptionIsNull()
    {
        // Arrange
        Exception exception = null;

        // Act
        string[] result = exception.GetHierarchicalMessages();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetHierarchicalMessages_ShouldReturnSingleMessage_WhenNoInnerException()
    {
        // Arrange
        var exception = new InvalidOperationException("boom");

        // Act
        string[] result = exception.GetHierarchicalMessages();

        // Assert
        result.Should().HaveCount(1);
        result[0].Should().Contain("boom");
    }

    [Fact]
    public void GetHierarchicalMessages_ShouldFlattenInnerExceptions()
    {
        // Arrange
        var inner = new ArgumentException("inner cause");
        var outer = new InvalidOperationException("outer", inner);

        // Act
        string[] result = outer.GetHierarchicalMessages();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(m => m.Contains("outer"));
        result.Should().Contain(m => m.Contains("inner cause"));
    }
}
