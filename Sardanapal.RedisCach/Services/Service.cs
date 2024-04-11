using System.Text.Json;
using StackExchange.Redis;
using Sardanapal.DomainModel.Domain;
using Sardanapal.ViewModel.Response;
using Sardanapal.ViewModel.Models;
using AutoMapper;
using Sardanapal.RedisCach.Models;

namespace Sardanapal.RedisCache.Services;

public abstract class CacheService<TModel, TKey, TSearchVM, TVM, TNewVM, TEditableVM>
    : ICacheService<TKey, TSearchVM, TVM, TNewVM, TEditableVM>
    where TModel : IBaseEntityModel<TKey>, new()
    where TKey : IEquatable<TKey>, IComparable<TKey>
    where TSearchVM : class, ICachModel<TKey>, new()
    where TVM : class, ICachModel<TKey>, new()
    where TNewVM : class, ICachModel<TKey>, new()
    where TEditableVM : class, ICachModel<TKey>, new()
{
    protected IConnectionMultiplexer connMultiplexer { get; set; }
    protected IMapper mapper { get; set; }
    protected int expireTime;

    protected abstract string key { get; }
    protected RedisKey rKey
    {
        get
        {
            return new RedisKey(key);
        }
    }

    public CacheService(IConnectionMultiplexer _connectionMultiplexer, IMapper _mapper, int _expireTime = 0)
    {
        expireTime = _expireTime;
        connMultiplexer = _connectionMultiplexer;
        mapper = _mapper;
    }

    protected virtual IDatabase GetCurrentDatabase()
    {
        return connMultiplexer.GetDatabase();
    }

    protected virtual IEnumerable<TModel> Search(IEnumerable<TModel> list, TSearchVM model)
    {
        return list;
    }

    public virtual async Task<IResponse<TVM>> Get(TKey id)
    {
        var result = new Response<TVM>(GetType().Name, OperationType.Fetch);

        try
        {
            string item = await GetCurrentDatabase()
                .HashGetAsync(rKey, new RedisValue(id.ToString()));

            if (!string.IsNullOrWhiteSpace(item))
            {
                var model = JsonSerializer.Deserialize<TModel>(item);
                var value = mapper.Map<TVM>(model);
                result.Set(StatusCode.Succeeded, value);
            }
        }
        catch (Exception ex)
        {
            result.Set(StatusCode.Exception, ex);
        }

        return result;
    }

    public virtual async Task<IResponse<GridVM<T, TSearchVM>>> GetAll<T>(GridSearchModelVM<TSearchVM> model = null)
        where T : class
    {
        var result = new Response<GridVM<T, TSearchVM>>(GetType().Name, OperationType.Fetch);

        return await result.FillAsync(async () =>
        {
            var resultValue = new GridVM<T, TSearchVM>(model);
            var items = await GetCurrentDatabase().HashGetAllAsync(rKey);

            if (items != null && items.Length > 0)
            {
                var enumerable = items
                    .Select(x => JsonSerializer.Deserialize<TModel>(x.Value));

                // TODO: Needs test
                var list = Search(enumerable, model.Fields)
                    .Select(x => mapper.Map<T>(x));

                resultValue.List = list.ToList();

                result.Set(StatusCode.Succeeded, resultValue!);
            }
            else
            {
                result.Set(StatusCode.NotExists);
            }

            return result;
        });
    }

    public virtual async Task<IResponse<TKey>> Add(TNewVM model)
    {
        var result = new Response<TKey>(GetType().Name, OperationType.Add);

        return await result.FillAsync(async () =>
        {
            TKey newId = model.Id;
            var added = await GetCurrentDatabase().HashSetAsync(rKey
                , new RedisValue(newId.ToString())
                , new RedisValue(JsonSerializer.Serialize(model)));

            bool setExpiration = true;

            if (expireTime > 0)
            {
                setExpiration = await GetCurrentDatabase()
                    .KeyExpireAsync(rKey, DateTime.UtcNow.AddMinutes(expireTime));
            }

            if (added && setExpiration)
                result.Set(StatusCode.Succeeded, newId);
            else
                result.Set(StatusCode.Failed);

            return result;
        });
    }

    public virtual async Task<IResponse<TEditableVM>> GetEditable(TKey id)
    {
        var result = new Response<TEditableVM>(GetType().Name, OperationType.Fetch);

        return await result.FillAsync(async () =>
        {
            string item = await GetCurrentDatabase()
                .HashGetAsync(rKey, new RedisValue(id.ToString()));

            if (!string.IsNullOrWhiteSpace(item))
            {
                var model = mapper.Map<TEditableVM>(JsonSerializer.Deserialize<TModel>(item));
                result.Set(StatusCode.Succeeded, model);
            }
            else
            {
                result.Set(StatusCode.NotExists);
            }

            return result;
        });
    }

    public virtual async Task<IResponse<bool>> Edit(TKey id, TEditableVM model)
    {
        var result = new Response<bool>(GetType().Name, OperationType.Edit);

        return await result.FillAsync(async () =>
        {
            var idValue = new RedisValue(id.ToString());

            string oldJson = await GetCurrentDatabase()
                .HashGetAsync(rKey, idValue);

            if (!string.IsNullOrWhiteSpace(oldJson))
            {
                var value = await GetCurrentDatabase()
                    .HashSetAsync(rKey
                        , idValue
                        , new RedisValue(JsonSerializer.Serialize(model)));

                result.Set(StatusCode.Succeeded, value);
            }
            else
            {
                result.Set(StatusCode.NotExists);
            }

            return result;
        });
    }

    public virtual async Task<IResponse<bool>> Delete(TKey id)
    {
        var result = new Response<bool>(GetType().Name, OperationType.Delete);

        return await result.FillAsync(async () =>
        {
            var old = await Get(id);
            if (old.StatusCode == StatusCode.Succeeded)
            {
                var value = await GetCurrentDatabase()
                    .HashDeleteAsync(rKey, new RedisValue(id.ToString()));
                result.Set(StatusCode.Succeeded, value);
            }
            else
            {
                result.Set(StatusCode.NotExists);
            }

            return result;
        });
    }

    public virtual async Task<IResponse<GridVM<SelectOptionVM<TKey, object>, TSearchVM>>> GetDictionary(GridSearchModelVM<TSearchVM> model = null)
    {
        var result = new Response<GridVM<SelectOptionVM<TKey, object>, TSearchVM>>(GetType().Name, OperationType.Fetch);

        return await result.FillAsync(async () =>
        {
            var resultValue = new GridVM<SelectOptionVM<TKey, object>, TSearchVM>(model);
            var items = await GetCurrentDatabase().HashGetAllAsync(rKey);

            if (items != null && items.Length > 0)
            {
                var enumerable = items
                    .Select(x => JsonSerializer.Deserialize<TModel>(x.Value));

                // TODO: Needs test
                var list = Search(enumerable, model.Fields)
                    .Select(x => mapper.Map<SelectOptionVM<TKey, object>>(x));

                resultValue.List = list.ToList();

                result.Set(StatusCode.Succeeded, resultValue!);
            }
            else
            {
                result.Set(StatusCode.NotExists);
            }

            return result;
        });
    }
}