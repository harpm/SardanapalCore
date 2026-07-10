using FluentAssertions;
using Sardanapal.Localization;
using Xunit;

namespace Sardanapal.Localization.Tests.Unit;

public class ResourceHelperTests
{
    [Fact]
    public void Messages_ShouldProvideNonEmptyCoreStrings()
    {
        Messages.InternalError.Should().NotBeNullOrEmpty();
        Messages.NotExist.Should().NotBeNullOrEmpty();
        Messages.OperationCancelled.Should().NotBeNullOrEmpty();
        Messages.NotFoundByKey.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData(5)]
    [InlineData("abc")]
    public void CreateNotFoundByKeyMessage_ShouldContainTheKey(object key)
    {
        // Act
        string message = ResourceHelper.CreateNotFoundByKeyMessage(key);

        // Assert
        message.Should().Contain(key.ToString());
    }

    [Fact]
    public void CreateRabbitMQMessageHandled_ShouldInterpolateIdAndDate()
    {
        // Act
        string message = ResourceHelper.CreateRabbitMQMessageHandled("42", "2024-01-02");

        // Assert
        message.Should().Contain("42");
        message.Should().Contain("2024-01-02");
    }

    [Fact]
    public void CreateRabbitMQMessagePublished_ShouldInterpolateIdAndDate()
    {
        // Act
        string message = ResourceHelper.CreateRabbitMQMessagePublished("7", "2024-03-04");

        // Assert
        message.Should().Contain("7");
        message.Should().Contain("2024-03-04");
    }
}
