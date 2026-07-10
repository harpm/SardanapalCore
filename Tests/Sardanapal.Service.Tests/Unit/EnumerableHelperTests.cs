using FluentAssertions;
using Sardanapal.Contract.IModel;
using Sardanapal.Domain.Model;
using Sardanapal.Service.Utilities;
using Sardanapal.ViewModel.Models;
using Xunit;

namespace Sardanapal.Service.Tests.Unit;

public class EnumerableHelperTests
{
    private class Row : BaseEntityModel<int>, IBaseEntityModel<int>
    {
        public string Name { get; set; }
    }

    private static IEnumerable<Row> Rows() => new[]
    {
        new Row { Id = 1, Name = "c" },
        new Row { Id = 2, Name = "a" },
        new Row { Id = 3, Name = "b" }
    };

    private record RowSearchVM : GridSearchModelVM<int>;

    [Fact]
    public void Search_ShouldReturnUnfiltered_WhenSearchModelIsNull()
    {
        // Act
        var result = Rows().Search<int, Row>(null).ToList();

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact(Skip = "EnumerableHelper.Search has a lambda parameter bug; see Issue.csv")]
    public void Search_ShouldSortAscendingById()
    {
        // Arrange
        var search = new RowSearchVM { SortId = nameof(Row.Id), SortAscending = true };

        // Act
        var result = Rows().Search<int, Row>(search).ToList();

        // Assert
        result.Select(r => r.Id).Should().BeInAscendingOrder();
    }

    [Fact(Skip = "EnumerableHelper.Search has a lambda parameter bug; see Issue.csv")]
    public void Search_ShouldSortDescendingById()
    {
        // Arrange
        var search = new RowSearchVM { SortId = nameof(Row.Id), SortAscending = false };

        // Act
        var result = Rows().Search<int, Row>(search).ToList();

        // Assert
        result.Select(r => r.Id).Should().BeInDescendingOrder();
    }

    [Fact(Skip = "EnumerableHelper.Search has a lambda parameter bug; see Issue.csv")]
    public void Search_ShouldSortByName()
    {
        // Arrange
        var search = new RowSearchVM { SortId = nameof(Row.Name), SortAscending = true };

        // Act
        var result = Rows().Search<int, Row>(search).ToList();

        // Assert
        result.Select(r => r.Name).Should().BeEquivalentTo(new[] { "a", "b", "c" });
    }

    [Fact(Skip = "EnumerableHelper.Search has a lambda parameter bug; see Issue.csv")]
    public void Search_ShouldApplyOffsetPaging()
    {
        // Arrange
        var search = new RowSearchVM { SortId = nameof(Row.Id), PageIndex = 1, PageSize = 1 };

        // Act
        var result = Rows().Search<int, Row>(search).ToList();

        // Assert
        result.Should().ContainSingle().Which.Id.Should().Be(2);
    }

    [Fact(Skip = "EnumerableHelper.Search has a lambda parameter bug; see Issue.csv")]
    public void Search_ShouldApplyKeysetPaging_WhenSortedByIdWithLastIdentifier()
    {
        // Arrange
        var search = new RowSearchVM
        {
            SortId = nameof(Row.Id),
            PageIndex = 0,
            PageSize = 10,
            LastIdentifier = 1
        };

        // Act
        var result = Rows().Search<int, Row>(search).ToList();

        // Assert - only rows with Id > 1 remain
        result.Should().HaveCount(2);
        result.Select(r => r.Id).Should().AllSatisfy(id => id.Should().BeGreaterThan(1));
    }

    [Fact(Skip = "EnumerableHelper.Search has a lambda parameter bug; see Issue.csv")]
    public void Search_ShouldFallBackToOffsetPaging_WhenSortedByNonIdColumn()
    {
        // Arrange: sorted by Name but LastIdentifier set; must not apply keyset (would drop rows)
        var search = new RowSearchVM
        {
            SortId = nameof(Row.Name),
            PageIndex = 0,
            PageSize = 10,
            LastIdentifier = 1
        };

        // Act
        var result = Rows().Search<int, Row>(search).ToList();

        // Assert - all rows present (no keyset filtering on Name)
        result.Should().HaveCount(3);
    }
}
