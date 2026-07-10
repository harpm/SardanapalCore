using FluentAssertions;
using Sardanapal.Share.Extensions;
using Xunit;

namespace Sardanapal.Share.Tests.Unit;

public class TypesExtensionsTests
{
    [Fact]
    public void ImplementsRawGeneric_ShouldReturnTrue_WhenTypeImplementsGenericInterface()
    {
        // Act
        bool result = typeof(List<int>).ImplementsRawGeneric(typeof(IEnumerable<>));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ImplementsRawGeneric_ShouldReturnFalse_WhenTypeDoesNotImplementInterface()
    {
        // Act
        bool result = typeof(int).ImplementsRawGeneric(typeof(IEnumerable<>));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSubClassOfRawGeneric_ShouldReturnTrue_WhenTypeIsGenericSubclass()
    {
        // Act
        bool result = typeof(Dictionary<,>).IsSubClassOfRawGeneric(typeof(Dictionary<int, string>));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSubClassOfRawGeneric_ShouldReturnFalse_ForUnrelatedType()
    {
        // Act
        bool result = typeof(Dictionary<,>).IsSubClassOfRawGeneric(typeof(List<int>));

        // Assert
        result.Should().BeFalse();
    }
}
