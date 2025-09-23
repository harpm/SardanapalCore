// Licensed under the MIT license.

using System.Text.Json;
using StackExchange.Redis;
using Sardanapal.Contract.IModel;
using Sardanapal.Contract.IRepository;

namespace Sardanapal.RedisCache;

public abstract class RedisRepository<TKey, TModel> : IMemoryRepository<TKey, TModel>
    where TKey : IComparable<TKey>, IEquatable<TKey>
    where TModel : class, IBaseEntityModel<TKey>, new()
{
    protected readonly IConnectionMultiplexer _conn;
    protected abstract string key { get; }
    protected RedisKey rKey
    {
        get
        {
            return new RedisKey(key);
        }
    }

    public RedisRepository(IConnectionMultiplexer conn)
    {
        this._conn = conn;
    }

    protected virtual IDatabase GetCurrentDatabase()
    {
        return _conn.GetDatabase();
    }

    protected virtual void GenerateId(TModel model)
    {
        return;
    }

    protected virtual Task GenerateIdAsync(TModel model)
    {
        return Task.CompletedTask;
    }

    public IEnumerable<TModel> FetchAll(CancellationToken ct = default)
    {
        var result = Enumerable.Empty<TModel>();

        var items = GetCurrentDatabase()
            .HashGetAll(rKey);

        if (items != null && items.Length > 0)
        {
            result = items.Select(x => JsonSerializer.Deserialize<TModel>(x.Value));
        }

        return result;
    }

    public Task<IEnumerable<TModel>> FetchAllAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public TModel FetchById(TKey id, CancellationToken ct = default)
    {
        EnsureNotNullReference(id);

        TModel result = default;

        string item = GetCurrentDatabase()
            .HashGet(rKey, new RedisValue(id.ToString()));

        if (!string.IsNullOrWhiteSpace(item))
        {
            result = JsonSerializer.Deserialize<TModel>(item);
        }

        return result;
    }

    public async Task<TModel> FetchByIdAsync(TKey id, CancellationToken ct = default)
    {
        EnsureNotNullReference(id);

        TModel result = default;

        string item = await GetCurrentDatabase()
            .HashGetAsync(rKey, new RedisValue(id.ToString()));

        if (!string.IsNullOrWhiteSpace(item))
        {
            result = JsonSerializer.Deserialize<TModel>(item);
        }

        return result;
    }

    public TKey Add(TModel model, CancellationToken ct = default)
    {
        EnsureNotNullReference(model);

        TKey result = default;

        var db = GetCurrentDatabase();
        GenerateId(model);
        var res = db.HashSet(rKey, new RedisValue(model.Id.ToString()), new RedisValue(JsonSerializer.Serialize(model)));

        if (res)
        {
            result = model.Id;
        }

        return result;
    }

    public async Task<TKey> AddAsync(TModel model, CancellationToken ct = default)
    {
        EnsureNotNullReference(model);

        TKey result = default;

        var db = GetCurrentDatabase();
        await GenerateIdAsync(model);
        var res = await db.HashSetAsync(rKey, new RedisValue(model.Id.ToString()), new RedisValue(JsonSerializer.Serialize(model)));

        if (res)
        {
            result = model.Id;
        }

        return result;
    }

    public bool Update(TKey id, TModel model, CancellationToken ct = default)
    {
        EnsureNotNullReference(id);

        var result = false;

        var value = GetCurrentDatabase().HashGet(rKey, new RedisValue(id.ToString()));
        if (string.IsNullOrWhiteSpace(value))
        {
            result = GetCurrentDatabase().HashSet(rKey, new RedisValue(id.ToString()), new RedisValue(JsonSerializer.Serialize(model)));
        }

        return result;
    }

    public async Task<bool> UpdateAsync(TKey id, TModel model, CancellationToken ct = default)
    {
        EnsureNotNullReference(id);

        var result = false;

        var value = await GetCurrentDatabase().HashGetAsync(rKey, new RedisValue(id.ToString()));
        if (string.IsNullOrWhiteSpace(value))
        {
            result = await GetCurrentDatabase().HashSetAsync(rKey, new RedisValue(id.ToString()), new RedisValue(JsonSerializer.Serialize(model)));
        }

        return result;
    }

    public void DeleteRange(IEnumerable<TKey> keys, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteRangeAsync(IEnumerable<TKey> keys, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public bool Delete(TKey id, CancellationToken ct = default)
    {
        EnsureNotNullReference(id);

        var result = false;

        var value = GetCurrentDatabase().HashGet(rKey, new RedisValue(id.ToString()));
        if (string.IsNullOrWhiteSpace(value))
        {
            result = GetCurrentDatabase().HashDelete(rKey, new RedisValue(id.ToString()));
        }

        return result;
    }

    public async Task<bool> DeleteAsync(TKey id, CancellationToken ct = default)
    {
        EnsureNotNullReference(id);

        var result = false;

        var value = await GetCurrentDatabase().HashGetAsync(rKey, new RedisValue(id.ToString()));
        if (string.IsNullOrWhiteSpace(value))
        {
            result = await GetCurrentDatabase().HashDeleteAsync(rKey, new RedisValue(id.ToString()));
        }

        return result;
    }

    protected void EnsureNotNullReference<T>(T values, CancellationToken ct = default)
    {
        if (values == null) throw new ArgumentNullException(nameof(values));
    }

    protected void EnsureNotNullCollection<T>(IEnumerable<T> values, CancellationToken ct = default)
    {
        if (values == null || values.Count() == 0) throw new ArgumentNullException(nameof(values));
    }
}
