using FluentAssertions;
using Sardanapal.Contract.IModel;
using Sardanapal.Domain.Model;
using Sardanapal.Service.Repository;
using Xunit;

namespace Sardanapal.Service.Tests.Unit;

public class MemoryRepositoryBaseTests
{
    private class Item : BaseEntityModel<int>, IBaseEntityModel<int>
    {
        public string Name { get; set; }
    }

    private class ItemRepository : MemoryRepositoryBase<int, Item>
    {
    }

    [Fact]
    public async Task AddAsync_ThenFetchByIdAsync_ShouldReturnStoredItem()
    {
        // Arrange
        var repo = new ItemRepository();
        var item = new Item { Id = 1, Name = "first" };

        // Act
        await repo.AddAsync(item);
        Item fetched = await repo.FetchByIdAsync(1);

        // Assert
        fetched.Should().NotBeNull();
        fetched.Name.Should().Be("first");
    }

    [Fact]
    public async Task FetchByIdAsync_ShouldReturnDefault_WhenKeyMissing()
    {
        // Arrange
        var repo = new ItemRepository();

        // Act
        Item fetched = await repo.FetchByIdAsync(999);

        // Assert
        fetched.Should().BeNull();
    }

    [Fact]
    public async Task FetchAllAsync_ShouldReturnAllStoredItems()
    {
        // Arrange
        var repo = new ItemRepository();
        await repo.AddAsync(new Item { Id = 1, Name = "a" });
        await repo.AddAsync(new Item { Id = 2, Name = "b" });

        // Act
        var all = (await repo.FetchAllAsync()).ToList();

        // Assert
        all.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnTrue_AndReplaceValue_WhenKeyExists()
    {
        // Arrange
        var repo = new ItemRepository();
        await repo.AddAsync(new Item { Id = 1, Name = "old" });

        // Act
        bool updated = await repo.UpdateAsync(1, new Item { Id = 1, Name = "new" });
        Item fetched = await repo.FetchByIdAsync(1);

        // Assert
        updated.Should().BeTrue();
        fetched.Name.Should().Be("new");
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenKeyMissing()
    {
        // Arrange
        var repo = new ItemRepository();

        // Act
        bool updated = await repo.UpdateAsync(42, new Item { Id = 42, Name = "x" });

        // Assert
        updated.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveItem()
    {
        // Arrange
        var repo = new ItemRepository();
        await repo.AddAsync(new Item { Id = 1, Name = "a" });

        // Act
        await repo.DeleteAsync(1);

        // Assert
        (await repo.FetchByIdAsync(1)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenKeyMissing()
    {
        // Arrange
        var repo = new ItemRepository();

        // Act
        Func<Task> act = () => repo.DeleteAsync(404);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteRangeAsync_ShouldRemoveAllSpecifiedKeys()
    {
        // Arrange
        var repo = new ItemRepository();
        await repo.AddAsync(new Item { Id = 1, Name = "a" });
        await repo.AddAsync(new Item { Id = 2, Name = "b" });
        await repo.AddAsync(new Item { Id = 3, Name = "c" });

        // Act
        await repo.DeleteRangeAsync(new[] { 1, 3 });

        // Assert
        var remaining = (await repo.FetchAllAsync()).Select(i => i.Id).ToList();
        remaining.Should().BeEquivalentTo(new[] { 2 });
    }
}
