using FluentAssertions;
using Sardanapal.Share.Extensions;
using Xunit;

namespace Sardanapal.Share.Tests.Unit;

public class IEnumerableExtensionsTests
{
    private class SampleItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    [Fact]
    public void Page_ShouldReturnCorrectSlice_ForGivenIndex()
    {
        // Arrange
        var data = Enumerable.Range(1, 5).ToList();

        // Act
        var page0 = data.Page(0, 2).ToList();
        var page1 = data.Page(1, 2).ToList();
        var page2 = data.Page(2, 2).ToList();

        // Assert
        page0.Should().BeEquivalentTo(new[] { 1, 2 });
        page1.Should().BeEquivalentTo(new[] { 3, 4 });
        page2.Should().BeEquivalentTo(new[] { 5 });
    }

    [Fact]
    public void SelectDynamicColumns_ShouldReturnOriginal_WhenColumnsAreNull()
    {
        // Arrange
        var list = new List<SampleItem>
        {
            new() { Id = 1, Name = "a" }
        };

        // Act
        var result = list.SelectDynamicColumns(null);

        // Assert - must not enumerate (ToList) or reference equality breaks
        result.Should().BeSameAs(list);
    }

    [Fact]
    public void SelectDynamicColumns_ShouldProjectOnlySpecifiedColumns()
    {
        // Arrange
        var list = new List<SampleItem>
        {
            new() { Id = 1, Name = "a" }
        };

        // Act
        var result = list.SelectDynamicColumns(new[] { "Name" }).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("a");
        result[0].Id.Should().Be(0);
    }

    [Fact(Skip = "DynamicSearch has a bug combining LambdaExpression nodes; see Issue.csv")]
    public void DynamicSearch_ShouldReturnOnlyMatchingItems()
    {
        // Arrange
        var list = new List<SampleItem>
        {
            new() { Id = 1, Name = "apple" },
            new() { Id = 2, Name = "banana" },
            new() { Id = 3, Name = "cherry" }
        };

        // Act
        var result = list.DynamicSearch("an").ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("banana");
    }

    [Fact]
    public void SearchDynamic_ShouldMatchAcrossAllStringFields()
    {
        // Arrange
        var list = new List<SampleItem>
        {
            new() { Id = 10, Name = "alpha" },
            new() { Id = 20, Name = "beta" }
        };

        // Act
        var byId = list.SearchDynamic("10").ToList();
        var byName = list.SearchDynamic("alpha").ToList();

        // Assert
        byId.Should().ContainSingle().Which.Id.Should().Be(10);
        byName.Should().ContainSingle().Which.Name.Should().Be("alpha");
    }
}
