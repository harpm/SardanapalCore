using FluentAssertions;
using StackExchange.Redis;
using Sardanapal.Contract.IModel;
using Sardanapal.RedisCache;
using Testcontainers.Redis;
using Xunit;

namespace Sardanapal.RedisCache.Tests.Integration;

public class RedisRepositoryTests : IAsyncLifetime
{
    private RedisContainer? _container;

    private IConnectionMultiplexer? _multiplexer;

    public async Task InitializeAsync()
    {
        _container = new RedisBuilder()
            .WithImage("redis:7.4")
            .Build();
        await _container.StartAsync();
        _multiplexer = await ConnectionMultiplexer.ConnectAsync(_container.GetConnectionString());
    }

    public async Task DisposeAsync()
    {
        _multiplexer?.Dispose();
        if (_container != null)
        {
            await _container.DisposeAsync();
        }
    }

    public class Item : IBaseEntityModel<int>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class ItemRedisRepository : RedisRepository<int, Item>
    {
        public ItemRedisRepository(IConnectionMultiplexer conn) : base(conn)
        {
        }

        protected override string key => "items";
    }

    [DockerFact]
    public async Task AddAsync_ThenFetchByIdAsync_ShouldRoundTrip()
    {
        // Arrange
        var repo = new ItemRedisRepository(_multiplexer!);

        // Act
        await repo.AddAsync(new Item { Id = 1, Name = "first" });
        Item fetched = await repo.FetchByIdAsync(1);

        // Assert
        fetched.Should().NotBeNull();
        fetched.Name.Should().Be("first");
    }

    [DockerFact]
    public async Task FetchByIdAsync_ShouldReturnNull_WhenKeyMissing()
    {
        // Arrange
        var repo = new ItemRedisRepository(_multiplexer!);

        // Act
        Item fetched = await repo.FetchByIdAsync(999);

        // Assert
        fetched.Should().BeNull();
    }

    [DockerFact]
    public async Task DeleteAsync_ShouldRemoveEntry()
    {
        // Arrange
        var repo = new ItemRedisRepository(_multiplexer!);
        await repo.AddAsync(new Item { Id = 1, Name = "a" });

        // Act
        await repo.DeleteAsync(1);

        // Assert
        (await repo.FetchByIdAsync(1)).Should().BeNull();
    }

    [DockerFact]
    public async Task DeleteAsync_ShouldThrow_WhenKeyMissing()
    {
        // Arrange
        var repo = new ItemRedisRepository(_multiplexer!);

        // Act
        Func<Task> act = () => repo.DeleteAsync(404);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
