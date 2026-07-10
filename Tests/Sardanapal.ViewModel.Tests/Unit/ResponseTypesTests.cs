using FluentAssertions;
using Sardanapal.ViewModel.Response;
using Xunit;

namespace Sardanapal.ViewModel.Tests.Unit;

public class ResponseTypesTests
{
    [Fact]
    public void StatusCode_ShouldHaveStableByteValues()
    {
        // Assert - values are part of the wire protocol and must stay stable
        ((byte)StatusCode.Succeeded).Should().Be(0);
        ((byte)StatusCode.Failed).Should().Be(1);
        ((byte)StatusCode.Canceled).Should().Be(3);
        ((byte)StatusCode.NotExists).Should().Be(4);
        ((byte)StatusCode.Exception).Should().Be(5);
        ((byte)StatusCode.Duplicate).Should().Be(6);
        ((byte)StatusCode.Forbidden).Should().Be(7);
    }

    [Fact]
    public void OperationType_ShouldHaveStableByteValues()
    {
        ((byte)OperationType.Fetch).Should().Be(0);
        ((byte)OperationType.Add).Should().Be(1);
        ((byte)OperationType.Edit).Should().Be(2);
        ((byte)OperationType.Delete).Should().Be(3);
        ((byte)OperationType.Function).Should().Be(4);
    }
}
