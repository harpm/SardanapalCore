using System.Text.Json;
using StackExchange.Redis;
using Sardanapal.DomainModel.Domain;
using Sardanapal.ViewModel.Response;

namespace Sardanapal.RedisCache.Services;

public abstract class CacheService<TModel, TNewVM, TEditableVM> : ICacheService<TModel, TNewVM, TEditableVM>
    where TModel : IBaseEntityModel<Guid>, new()
    where TNewVM : class
    where TEditableVM : class
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

    public virtual async Task<IResponse<TModel>> Get(Guid Id)
    {
        var result = new Response<TModel>(this.GetType().Name, OperationType.Fetch);

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

    public virtual async Task<IResponse<IEnumerable<TModel>>> GetAll()
    {
        var result = new Response<IEnumerable<TModel>>(this.GetType().Name, OperationType.Fetch);

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

    public virtual async Task<IResponse<Guid>> Add(TNewVM Model)
    {
        var result = new Response<Guid>(this.GetType().Name, OperationType.Add);

        try
        {
            var newId = Guid.NewGuid();
            var rKey = new RedisKey(Key);
            var added = await Db.HashSetAsync(rKey
                , new RedisValue(newId.ToString())
                , new RedisValue(JsonSerializer.Serialize(Model)));

            bool setExpiration = true;

            if (this.expireTime > 0)
            {
                setExpiration = await Db.KeyExpireAsync(rKey, DateTime.UtcNow.AddMinutes(this.expireTime));
            }

            if (added && setExpiration)
                result.Set(StatusCode.Succeeded, newId);
            else
                result.Set(StatusCode.Failed);

        }
        catch (Exception ex)
        {
            result.Set(StatusCode.Exception, ex);
        }

        return result;
    }

    public virtual async Task<IResponse<bool>> Edit(Guid Id, TEditableVM Model)
    {
        var result = new Response<bool>(this.GetType().Name, OperationType.Edit);
        try
        {
            var old = await Get(Id);
            if (old.StatusCode == StatusCode.Succeeded)
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

    public virtual async Task<IResponse<bool>> Delete(Guid Id)
    {
        var result = new Response<bool>(this.GetType().Name, OperationType.Delete);

        try
        {
            var old = await Get(Id);
            if (old.StatusCode == StatusCode.Succeeded)
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