using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Sardanapal.ViewModel.Response;
using Xunit;

namespace Sardanapal.ViewModel.Tests.Unit;

public class ResponseTests
{
    private static ILogger Logger() => Substitute.For<ILogger>();

    [Fact]
    public void Constructor_ShouldDefaultServiceNameToDefault()
    {
        // Act
        IResponse<int> response = new Response<int>(Logger());

        // Assert
        response.ServiceName.Should().Be("Default");
    }

    [Fact]
    public void Constructor_ShouldUseProvidedServiceNameAndOperationType()
    {
        // Act
        IResponse<int> response = new Response<int>("Orders", OperationType.Add, Logger());

        // Assert
        response.ServiceName.Should().Be("Orders");
        response.OperationType.Should().Be(OperationType.Add);
    }

    [Fact]
    public void IsSuccess_ShouldBeTrue_WhenStatusCodeIsSucceeded()
    {
        // Arrange
        IResponse<int> response = new Response<int>(Logger());

        // Act
        response.Set(StatusCode.Succeeded);

        // Assert
        response.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void IsSuccess_ShouldBeFalse_WhenStatusCodeIsNotSucceeded()
    {
        // Arrange
        IResponse<int> response = new Response<int>(Logger());

        // Act
        response.Set(StatusCode.Failed);

        // Assert
        response.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Set_WithStatusCodeAndData_ShouldAssignBoth()
    {
        // Arrange
        IResponse<int> response = new Response<int>(Logger());

        // Act
        response.Set(StatusCode.Succeeded, 42);

        // Assert
        response.StatusCode.Should().Be(StatusCode.Succeeded);
        ((IResponse<int>)response).Data.Should().Be(42);
    }

    [Fact]
    public void Set_WithException_ShouldCaptureHierarchicalMessages()
    {
        // Arrange
        IResponse<int> response = new Response<int>(Logger());
        var exception = new InvalidOperationException("kaboom", new ArgumentException("root"));

        // Act
        response.Set(StatusCode.Exception, exception);

        // Assert
        response.StatusCode.Should().Be(StatusCode.Exception);
        response.DeveloperMessages.Should().HaveCount(2);
        response.DeveloperMessages.Should().Contain(m => m.Contains("kaboom"));
        response.DeveloperMessages.Should().Contain(m => m.Contains("root"));
    }

    [Fact]
    public void Fill_ShouldRunBodyAndPreserveSuccess()
    {
        // Arrange
        IResponse<int> response = new Response<int>("Srv", OperationType.Function, Logger());

        // Act
        IResponse<int> result = response.Fill(() => response.Set(StatusCode.Succeeded, 7));

        // Assert
        result.Should().BeSameAs(response);
        result.StatusCode.Should().Be(StatusCode.Succeeded);
        result.Data.Should().Be(7);
    }

    [Fact]
    public async Task FillAsync_ShouldMapExceptionToExceptionStatus()
    {
        // Arrange
        IResponse<int> response = new Response<int>("Srv", OperationType.Function, Logger());

        // Act
        IResponse<int> result = await response.FillAsync(() => throw new InvalidOperationException("fail"));

        // Assert
        result.StatusCode.Should().Be(StatusCode.Exception);
        result.DeveloperMessages.Should().NotBeEmpty();
        result.UserMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task FillAsync_ShouldMapOperationCanceledToCanceledStatus()
    {
        // Arrange
        IResponse<int> response = new Response<int>("Srv", OperationType.Function, Logger());

        // Act
        IResponse<int> result = await response.FillAsync(() => throw new OperationCanceledException());

        // Assert
        result.StatusCode.Should().Be(StatusCode.Canceled);
    }

    [Fact]
    public void ConvertTo_ShouldCopyAllFieldsToTarget()
    {
        // Arrange
        IResponse<int> source = new Response<int>("Src", OperationType.Edit, Logger());
        source.Set(StatusCode.NotExists, new[] { "missing" }, "not found");
        IResponse<int> target = new Response<int>(Logger());

        // Act
        source.ConvertTo<IResponse>(target);

        // Assert
        target.StatusCode.Should().Be(StatusCode.NotExists);
        target.ServiceName.Should().Be("Src");
        target.OperationType.Should().Be(OperationType.Edit);
        target.DeveloperMessages.Should().BeEquivalentTo(new[] { "missing" });
        target.UserMessage.Should().Be("not found");
    }
}
