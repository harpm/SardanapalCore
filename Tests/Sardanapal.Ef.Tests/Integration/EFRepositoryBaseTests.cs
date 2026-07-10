using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Sardanapal.Contract.IModel;
using Sardanapal.Domain.Model;
using Sardanapal.Ef.Repository;
using Xunit;

namespace Sardanapal.Ef.Tests.Integration;

public class EFRepositoryBaseTests
{
    public class Item : BaseEntityModel<int>, IBaseEntityModel<int>
    {
        public string Name { get; set; }
    }

    public class LogicalItem : LogicalBaseEntityModel<int>, IBaseEntityModel<int>, ILogicalEntityModel
    {
        public string Name { get; set; }
    }

    public class TestDbContext : DbContext
    {
        public DbSet<Item> Items => Set<Item>();
        public DbSet<LogicalItem> LogicalItems => Set<LogicalItem>();

        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }
    }

    public class ItemRepository : EFRepositoryBase<TestDbContext, int, Item>
    {
        public ItemRepository(TestDbContext context) : base(context)
        {
        }
    }

    public class LogicalItemRepository : EFRepositoryBase<TestDbContext, int, LogicalItem>
    {
        public LogicalItemRepository(TestDbContext context) : base(context)
        {
        }
    }

    private static async Task<(TestDbContext ctx, ItemRepository repo)> CreateItemRepoAsync(string dbName)
    {
        var (ctx, _) = await CreateContextAsync(dbName);
        return (ctx, new ItemRepository(ctx));
    }

    private static async Task<(TestDbContext ctx, LogicalItemRepository repo)> CreateLogicalRepoAsync(string dbName)
    {
        var (ctx, _) = await CreateContextAsync(dbName);
        return (ctx, new LogicalItemRepository(ctx));
    }

    private static async Task<(TestDbContext ctx, SqliteConnection conn)> CreateContextAsync(string dbName)
    {
        // SQLite in-memory is a relational provider, so ExecuteDelete/ExecuteUpdate
        // (used by DeleteRange soft/hard delete) are supported — unlike EF InMemory.
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(connection)
            .Options;
        var ctx = new TestDbContext(options);
        await ctx.Database.EnsureCreatedAsync();
        return (ctx, connection);
    }

    [Fact]
    public async Task AddAsync_ThenFetch_ShouldRoundTrip()
    {
        // Arrange
        var (ctx, repo) = await CreateItemRepoAsync(nameof(AddAsync_ThenFetch_ShouldRoundTrip));

        // Act
        await repo.AddAsync(new Item { Id = 1, Name = "first" });
        await ctx.SaveChangesAsync();
        Item fetched = await repo.FetchByIdAsync(1);

        // Assert
        fetched.Should().NotBeNull();
        fetched.Name.Should().Be("first");
    }

    [Fact]
    public async Task FetchByIdAsync_ShouldReturnNull_WhenMissing()
    {
        // Arrange
        var (_, repo) = await CreateItemRepoAsync(nameof(FetchByIdAsync_ShouldReturnNull_WhenMissing));

        // Act
        Item fetched = await repo.FetchByIdAsync(99);

        // Assert
        fetched.Should().BeNull();
    }

    [Fact]
    public async Task FetchAll_ShouldReturnAllCommittedEntities()
    {
        // Arrange
        var (ctx, repo) = await CreateItemRepoAsync(nameof(FetchAll_ShouldReturnAllCommittedEntities));
        await repo.AddAsync(new Item { Id = 1, Name = "a" });
        await repo.AddAsync(new Item { Id = 2, Name = "b" });
        await ctx.SaveChangesAsync();

        // Act
        var all = await repo.FetchAllAsync();
        var list = await all.ToListAsync();

        // Assert
        list.Should().HaveCount(2);
    }

    [Fact]
    public async Task Update_ShouldMarkEntityModified()
    {
        // Arrange
        var (ctx, repo) = await CreateItemRepoAsync(nameof(Update_ShouldMarkEntityModified));
        var item = new Item { Id = 1, Name = "a" };
        await repo.AddAsync(item);
        await ctx.SaveChangesAsync();

        // Act
        bool result = await repo.UpdateAsync(1, item);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_ShouldRemoveEntity()
    {
        // Arrange
        var (ctx, repo) = await CreateItemRepoAsync(nameof(Delete_ShouldRemoveEntity));
        await repo.AddAsync(new Item { Id = 1, Name = "a" });
        await ctx.SaveChangesAsync();

        // Act
        await repo.DeleteAsync(1);
        await ctx.SaveChangesAsync();

        // Assert
        (await repo.FetchByIdAsync(1)).Should().BeNull();
    }

    [Fact]
    public async Task Delete_ShouldThrowKeyNotFound_WhenMissing()
    {
        // Arrange
        var (_, repo) = await CreateItemRepoAsync(nameof(Delete_ShouldThrowKeyNotFound_WhenMissing));

        // Act
        Func<Task> act = () => repo.DeleteAsync(404);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteRangeAsync_ShouldHardDeleteNonLogicalEntity()
    {
        // Arrange
        var (ctx, repo) = await CreateItemRepoAsync(nameof(DeleteRangeAsync_ShouldHardDeleteNonLogicalEntity));
        await repo.AddAsync(new Item { Id = 1, Name = "a" });
        await repo.AddAsync(new Item { Id = 2, Name = "b" });
        await ctx.SaveChangesAsync();

        // Act
        await repo.DeleteRangeAsync(new[] { 1 });

        // Assert
        var remaining = await ctx.Items.ToListAsync();
        remaining.Should().ContainSingle().Which.Id.Should().Be(2);
    }

    [Fact]
    public async Task DeleteRangeAsync_ShouldSoftDeleteLogicalEntity()
    {
        // Arrange - the critical soft-delete branch must set IsDeleted instead of removing
        var (ctx, repo) = await CreateLogicalRepoAsync(nameof(DeleteRangeAsync_ShouldSoftDeleteLogicalEntity));
        await repo.AddAsync(new LogicalItem { Id = 1, Name = "a", IsDeleted = false });
        await repo.AddAsync(new LogicalItem { Id = 2, Name = "b", IsDeleted = false });
        await ctx.SaveChangesAsync();

        // Act
        await repo.DeleteRangeAsync(new[] { 1 });

        // Assert - both rows remain; only the targeted one is flagged IsDeleted.
        // AsNoTracking() bypasses the change tracker, which holds stale pre-ExecuteUpdate state.
        var all = await ctx.LogicalItems.AsNoTracking().ToListAsync();
        all.Should().HaveCount(2);
        all.Single(i => i.Id == 1).IsDeleted.Should().BeTrue();
        all.Single(i => i.Id == 2).IsDeleted.Should().BeFalse();
    }
}
