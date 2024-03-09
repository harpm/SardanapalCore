using System.Text.Json;
using StackExchange.Redis;
using Sardanapal.RedisCache.Models;
using Sardanapal.DomainModel.Domain;
using Sardanapal.ViewModel.Response;

namespace Sardanapal.RedisCache.Services;

public abstract class CacheService<TKey, TModel> : ICacheService<TKey, TModel>
    where TKey : IEquatable<TKey>, IComparable<TKey>
    where TModel : IBaseEntityModel<TKey>, new()
{
    protected IConnectionMultiplexer ConnMultiplexer { get; set; }
    protected IDatabase Db { get; set; }
    protected int expireTime;

    protected abstract string Key { get; }

    public CacheService(IConnectionMultiplexer connectionMultiplexer, int expireTime = 0)
    {
        this.expireTime = expireTime;
        ConnMultiplexer = connectionMultiplexer;
        Db = ConnMultiplexer.GetDatabase();
    }

    public virtual async Task<CacheResponse<TModel>> Get(TKey Id)
    {
        var result = new CacheResponse<TModel>(this.GetType().Name, OperationType.Fetch);

        try
        {
            var rKey = new RedisKey(Key);

            string item = await Db.HashGetAsync(rKey, new RedisValue(Id.ToString()));

            if (!string.IsNullOrWhiteSpace(item))
            {
                var Value = JsonSerializer.Deserialize<TModel>(item);
                result.Set(StatusCode.Succeeded, Value);
            }
        }
        catch (Exception ex)
        {
            result.Set(StatusCode.Exception, ex);
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
                result.Set(StatusCode.Succeeded, value!);
            }
            else
            {
                result.Set(StatusCode.NotExists);
            }
        }
        catch (Exception ex)
        {
            result.Set(StatusCode.Exception, ex);
        }

        return result;
    }

    public virtual async Task<CacheResponse<TKey>> Add(TModel Model)
    {
        var result = new CacheResponse<TKey>(this.GetType().Name, OperationType.Add);

        try
        {
            var old = await Get(Model.Id);
            if (old.Status != StatusCode.Succeeded)
            {
                var rKey = new RedisKey(Key);
                var added = await Db.HashSetAsync(rKey
                    , new RedisValue(Model.Id.ToString())
                    , new RedisValue(JsonSerializer.Serialize(Model)));

                bool setExpiration = true;

                if (this.expireTime > 0)
                {
                    setExpiration = await Db.KeyExpireAsync(rKey, DateTime.UtcNow.AddMinutes(this.expireTime));
                }

                if (added && setExpiration)
                    result.Set(StatusCode.Succeeded, Model.Id);
                else
                    result.Set(StatusCode.Failed);
            }
            else
            {
                result.Set(StatusCode.Failed);
                result.Messages = new[] { "The Item has been already added!" };
            }
        }
        catch (Exception ex)
        {
            result.Set(StatusCode.Exception, ex);
        }

        return result;
    }

    public virtual async Task<CacheResponse<bool>> Edit(TKey Id, TModel Model)
    {
        var result = new CacheResponse<bool>(this.GetType().Name, OperationType.Edit);
        try
        {
            var old = await Get(Model.Id);
            if (old.Status == StatusCode.Succeeded)
            {
                var value = await Db.HashSetAsync(new RedisKey(Key)
                    , new RedisValue(Id.ToString())
                    , new RedisValue(JsonSerializer.Serialize(Model)));

                result.Set(StatusCode.Succeeded, value);
            }
            else
            {
                result.Set(StatusCode.NotExists);
            }
        }
        catch (Exception ex)
        {
            result.Set(StatusCode.Exception, ex);
        }

        return result;
    }

    public virtual async Task<CacheResponse<bool>> Delete(TKey Id)
    {
        var result = new CacheResponse<bool>(this.GetType().Name, OperationType.Delete);

        try
        {
            var old = await Get(Id);
            if (old.Status == StatusCode.Succeeded)
            {
                var value = await Db.HashDeleteAsync(new RedisKey(Key), new RedisValue(Id.ToString()));
                result.Set(StatusCode.Succeeded, value);
            }
            else
            {
                result.Set(StatusCode.NotExists);
            }
        }
        catch (Exception ex)
        {
            result.Set(StatusCode.Exception, ex);
        }

        return result;
    }
}