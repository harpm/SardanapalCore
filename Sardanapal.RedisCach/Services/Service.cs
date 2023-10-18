using System.Text.Json;
using Sardanapal.ModelBase.Model.Domain;
using StackExchange.Redis;

namespace Sardanapal.RedisCach.Services;

public class CachService<TKey, TModel> : ICachService<TKey, TModel>
    where TKey : IEquatable<TKey>, IComparable<TKey>
    where TModel : IBaseEntityModel<TKey>, new()
{
    public IConnectionMultiplexer ConnectionMulti { get; set; }
    public IDatabase DB { get; set; } 
    
    protected virtual string Key => "key";
    
    public CachService(IConnectionMultiplexer _connectionMultiplexer)
    {
        ConnectionMulti = _connectionMultiplexer;
        DB = ConnectionMulti.GetDatabase();
    }
    
    public virtual async Task<TModel> Get(TKey Id)
    {
        string item = await DB.HashGetAsync(new RedisKey(Key), new RedisValue(Id.ToString()));
        return JsonSerializer.Deserialize<TModel>(item);
    }

    public virtual async Task<IEnumerable<TModel>> GetAll()
    {
        var items = await DB.HashGetAllAsync(new RedisKey(Key));
        return items.Select(x => JsonSerializer.Deserialize<TModel>(x.Value)).AsEnumerable();
    }

    public virtual async Task<TKey> Add(TModel Model)
    {
        await DB.HashSetAsync(new RedisKey(Key)
            , new RedisValue(Model.Id.ToString())
            , new RedisValue(JsonSerializer.Serialize(Model)));
        return Model.Id;
    }

    public virtual async Task<bool> Edit(TKey Id, TModel Model)
    {
        return await DB.HashSetAsync(new RedisKey(Key)
            , new RedisValue(Id.ToString())
            , new RedisValue(JsonSerializer.Serialize(Model)));
    }

    public virtual async Task<bool> Delete(TKey Id)
    {
        return await DB.HashDeleteAsync(new RedisKey(Key), new RedisValue(Id.ToString()));
    }
}