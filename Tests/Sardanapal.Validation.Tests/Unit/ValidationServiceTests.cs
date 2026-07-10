using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Sardanapal.Validation.Service;
using Xunit;

namespace Sardanapal.Validation.Tests.Unit;

public class ValidationServiceTests
{
    public class Sample
    {
        public string Value { get; set; }
    }

    public class SampleValidator : AbstractValidator<Sample>
    {
        public SampleValidator()
        {
            RuleFor(x => x.Value).NotEmpty();
        }
    }

    private static IServiceProvider ServiceProviderWithValidator(IValidator validator)
    {
        var sp = Substitute.For<IServiceProvider>();
        sp.GetService(typeof(IValidator<Sample>)).Returns(validator);
        return sp;
    }

    [Fact]
    public async Task ValidateParams_ShouldRemainValid_WhenNoValidatorRegistered()
    {
        // Arrange
        var sp = Substitute.For<IServiceProvider>();
        var service = new ValidationService(sp);

        // Act
        await service.ValidateParams(new[] { typeof(Sample) }, new object[] { new Sample { Value = "ok" } });

        // Assert
        service.IsValid.Should().BeTrue();
        service.Messages.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateParams_ShouldBeInvalid_WhenLengthsMismatch()
    {
        // Arrange
        var sp = Substitute.For<IServiceProvider>();
        var service = new ValidationService(sp);

        // Act
        await service.ValidateParams(new[] { typeof(Sample) }, new object[] { new Sample(), new Sample() });

        // Assert
        service.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateParams_ShouldBeInvalid_WhenValidatorFails()
    {
        // Arrange
        var sp = ServiceProviderWithValidator(new SampleValidator());
        var service = new ValidationService(sp);

        // Act
        await service.ValidateParams(new[] { typeof(Sample) }, new object[] { new Sample { Value = "" } });

        // Assert
        service.IsValid.Should().BeFalse();
        service.Messages.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ValidateParams_ShouldStayValid_WhenValidatorPasses()
    {
        // Arrange
        var sp = ServiceProviderWithValidator(new SampleValidator());
        var service = new ValidationService(sp);

        // Act
        await service.ValidateParams(new[] { typeof(Sample) }, new object[] { new Sample { Value = "ok" } });

        // Assert
        service.IsValid.Should().BeTrue();
        service.Messages.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateParams_ShouldNoopOnSecondCall_DueToProceededGuard()
    {
        // Arrange
        var sp = ServiceProviderWithValidator(new SampleValidator());
        var service = new ValidationService(sp);

        // Act
        await service.ValidateParams(new[] { typeof(Sample) }, new object[] { new Sample { Value = "" } });
        service.IsValid.Should().BeFalse();

        // Second call must not change state (IsProceeded guard)
        await service.ValidateParams(new[] { typeof(Sample) }, new object[] { new Sample { Value = "ok" } });

        // Assert
        service.IsProceeded.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateParams_ShouldHandleNullArraysGracefully()
    {
        // Arrange
        var sp = Substitute.For<IServiceProvider>();
        var service = new ValidationService(sp);

        // Act
        await service.ValidateParams(null, null);

        // Assert
        service.IsValid.Should().BeTrue();
        service.IsProceeded.Should().BeTrue();
    }
}
