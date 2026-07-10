using FluentAssertions;
using Sardanapal.Domain.Model;
using Xunit;

namespace Sardanapal.Domain.Tests.Unit;

public class EntityModelTests
{
    private class IntEntity : BaseEntityModel<int>
    {
        public string Title { get; set; }
    }

    private class IntLogicalEntity : LogicalBaseEntityModel<int>
    {
        public string Title { get; set; }
    }

    [Fact]
    public void BaseEntityModel_ShouldExposeTypedId()
    {
        // Arrange
        var entity = new IntEntity();

        // Act
        entity.Id = 5;

        // Assert
        entity.Id.Should().Be(5);
    }

    [Fact]
    public void LogicalEntityModel_ShouldDefaultIsDeletedToFalse()
    {
        // Arrange
        var entity = new IntLogicalEntity();

        // Act
        bool isDeleted = entity.IsDeleted;

        // Assert
        isDeleted.Should().BeFalse();
    }

    [Fact]
    public void LogicalEntityModel_ShouldAllowSettingIsDeleted()
    {
        // Arrange
        var entity = new IntLogicalEntity();

        // Act
        entity.IsDeleted = true;

        // Assert
        entity.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void EntityModel_ShouldTrackAuditFields()
    {
        // Arrange
        var entity = new AuditableEntity();

        // Act
        entity.CreateBy = 1;
        entity.ModifiedBy = 2;
        entity.CreatedOnUtc = new DateTime(2024, 1, 1);
        entity.ModifiedOnUtc = new DateTime(2024, 2, 1);

        // Assert
        entity.CreateBy.Should().Be(1);
        entity.ModifiedBy.Should().Be(2);
        entity.CreatedOnUtc.Should().Be(new DateTime(2024, 1, 1));
    }

    private class AuditableEntity : EntityModel<int, int>
    {
        public string Title { get; set; }
    }
}
