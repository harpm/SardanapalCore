using System.Text.Json;
using Sardanapal.ModelBase.Model.Domain;
using Sardanapal.ModelBase.Model.Types;
using StackExchange.Redis;
using Sardanapal.RedisCache.Models;

namespace Sardanapal.RedisCache.Services;

public class CacheService<TKey, TModel> : ICacheService<TKey, TModel>
    where TKey : IEquatable<TKey>, IComparable<TKey>
    where TModel : IBaseEntityModel<TKey>, new()
{
    protected IConnectionMultiplexer ConnMultiplexer { get; set; }
    protected IDatabase Db { get; set; }

    protected virtual string Key => "key";

    public CacheService(IConnectionMultiplexer connectionMultiplexer)
    {
        ConnMultiplexer = connectionMultiplexer;
        Db = ConnMultiplexer.GetDatabase();
    }

    public virtual async Task<CacheResponse<TModel>> Get(TKey Id)
    {
        var result = new CacheResponse<TModel>(this.GetType().Name, OperationType.Fetch);

        try
        {
            string item = await Db.HashGetAsync(new RedisKey(Key), new RedisValue(Id.ToString()));

            if (!string.IsNullOrWhiteSpace(item))
            {
                var Value = JsonSerializer.Deserialize<TModel>(item);
                result.Set(CacheStatusCode.Succeeded, Value);
            }
        }
        catch (Exception ex)
        {
            result.Set(CacheStatusCode.Exception, ex);
        }

        return result;
    }

    public virtual async Task<CacheResponse<IEnumerable<TModel>>> GetAll()
    {
        var result = new CacheResponse<IEnumerable<TModel>>(this.GetType().Name, OperationType.Fetch);

        try
        {
            var items = await Db.HashGetAllAsync(new RedisKey(Key));

            if (items != null && items.Length > 0)
            {
                var value = items
                    .Select(x => JsonSerializer.Deserialize<TModel>(x.Value))
                    .AsEnumerable();
                result.Set(CacheStatusCode.Succeeded, value!);
            }
            else
            {
                result.Set(CacheStatusCode.NotExist);
            }
        }
        catch (Exception ex)
        {
            result.Set(CacheStatusCode.Exception, ex);
        }

        return result;
    }

    public virtual async Task<CacheResponse<TKey>> Add(TModel Model)
    {
        var result = new CacheResponse<TKey>(this.GetType().Name, OperationType.Add);

        try
        {
            var old = await Get(Model.Id);
            if (old.Status != CacheStatusCode.Succeeded)
            {
                var added = await Db.HashSetAsync(new RedisKey(Key)
                    , new RedisValue(Model.Id.ToString())
                    , new RedisValue(JsonSerializer.Serialize(Model)));

                if (added)
                    result.Set(CacheStatusCode.Succeeded, Model.Id);
                else
                    result.Set(CacheStatusCode.Failed);
            }
            else
            {
                result.Set(CacheStatusCode.Failed);
                result.Messages = new[] { "The Item has been already added!" };
            }
        }
        catch (Exception ex)
        {
            result.Set(CacheStatusCode.Exception, ex);
        }

        return result;
    }

    public virtual async Task<CacheResponse<bool>> Edit(TKey Id, TModel Model)
    {
        var result = new CacheResponse<bool>(this.GetType().Name, OperationType.Edit);
        try
        {
            var old = await Get(Model.Id);
            if (old.Status == CacheStatusCode.Succeeded)
            {
                var value = await Db.HashSetAsync(new RedisKey(Key)
                    , new RedisValue(Id.ToString())
                    , new RedisValue(JsonSerializer.Serialize(Model)));

                result.Set(CacheStatusCode.Succeeded, value);
            }
            else
            {
                result.Set(CacheStatusCode.NotExist);
            }
        }
        catch (Exception ex)
        {
            result.Set(CacheStatusCode.Exception, ex);
        }

        return result;
    }

    public virtual async Task<CacheResponse<bool>> Delete(TKey Id)
    {
        var result = new CacheResponse<bool>(this.GetType().Name, OperationType.Delete);

        try
        {
            var old = await Get(Id);
            if (old.Status == CacheStatusCode.Succeeded)
            {
                var value = await Db.HashDeleteAsync(new RedisKey(Key), new RedisValue(Id.ToString()));
                result.Set(CacheStatusCode.Succeeded, value);
            }
            else
            {
                result.Set(CacheStatusCode.NotExist);
            }
        }
        catch (Exception ex)
        {
            result.Set(CacheStatusCode.Exception, ex);
        }

        return result;
    }
}