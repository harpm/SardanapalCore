using AutoMapper;
using System.Text.Json;
using StackExchange.Redis;
using Sardanapal.Contract.IModel;
using Sardanapal.ViewModel.Response;
using Sardanapal.ViewModel.Models;
using Sardanapal.Contract.IService;

namespace Sardanapal.RedisCache.Services;

public abstract class CacheService<TModel, TKey, TSearchVM, TVM, TNewVM, TEditableVM>
    : ICacheService<TModel, TKey, TSearchVM, TVM, TNewVM, TEditableVM>
    where TModel : IBaseEntityModel<TKey>, new()
    where TKey : IEquatable<TKey>, IComparable<TKey>
    where TSearchVM : class, new()
    where TVM : class, ICachModel<TKey>, new()
    where TNewVM : class, ICachModel<TKey>, new()
    where TEditableVM : class, ICachModel<TKey>, new()
{
    public virtual string ServiceName => "ServiceName";

    protected IConnectionMultiplexer connMultiplexer { get; set; }
    protected IMapper mapper { get; set; }
    protected virtual int expireTime { get; set; }

    protected abstract string key { get; }
    protected RedisKey rKey
    {
        get
        {
            return new RedisKey(key);
        }
    }

    public CacheService(IConnectionMultiplexer _connectionMultiplexer, IMapper _mapper)
    {
        connMultiplexer = _connectionMultiplexer;
        mapper = _mapper;
    }

    protected virtual IDatabase GetCurrentDatabase()
    {
        return connMultiplexer.GetDatabase();
    }

    protected async virtual Task<IEnumerable<TModel>> InternalGetAll()
    {
        var result = Enumerable.Empty<TModel>();

        var items = await GetCurrentDatabase().HashGetAllAsync(rKey);

        if (items != null && items.Length > 0)
        {
            result = items.Select(x => JsonSerializer.Deserialize<TModel>(x.Value));
        }

        return result;
    }

    protected virtual IEnumerable<TModel> Search(IEnumerable<TModel> list, TSearchVM model)
    {
        return list;
    }

    public virtual async Task<IResponse<TVM>> Get(TKey id)
    {
        var result = new Response<TVM>(GetType().Name, OperationType.Fetch);

        await result.FillAsync(async () =>
        {
            string item = await GetCurrentDatabase()
                .HashGetAsync(rKey, new RedisValue(id.ToString()));

            if (!string.IsNullOrWhiteSpace(item))
            {
                var model = JsonSerializer.Deserialize<TModel>(item);
                var value = mapper.Map<TVM>(model);
                result.Set(StatusCode.Succeeded, value);
            }
            else
            {
                result.Set(StatusCode.NotExists);
            }
        });
        return result;
    }

    public virtual async Task<IResponse<GridVM<TKey, T>>> GetAll<T>(GridSearchModelVM<TKey, TSearchVM> model = null)
        where T : class
    {
        var result = new Response<GridVM<TKey, T>>(GetType().Name, OperationType.Fetch);

        return await result.FillAsync(async () =>
        {
            var resultValue = new GridVM<TKey, T>(model);
            var items = await InternalGetAll();
            var list = Search(items, model.Fields)
                .Select(x => mapper.Map<T>(x));

            resultValue.SearchModel.TotalCount = items.Count();
            resultValue.List = list.ToList();

            result.Set(StatusCode.Succeeded, resultValue!);
        });
    }

    public virtual async Task<IResponse<TKey>> Add(TNewVM model)
    {
        var result = new Response<TKey>(GetType().Name, OperationType.Add);

        return await result.FillAsync(async () =>
        {
            TKey newId = model.Id;

            var newItem = mapper.Map<TNewVM, TModel>(model);

            var added = await GetCurrentDatabase().HashSetAsync(rKey
                , new RedisValue(newId.ToString())
                , new RedisValue(JsonSerializer.Serialize(newItem)));

            bool setExpiration = true;

            if (expireTime > 0)
            {
                await GetCurrentDatabase().ExecuteAsync($"HEXPIRE {key} {expireTime * 60} FIELDS 1 {newId.ToString()}");
            }

            result.Set(StatusCode.Succeeded, newId);
        });
    }

    public virtual async Task<IResponse<TKey>> Add(TModel model)
    {
        var result = new Response<TKey>(GetType().Name, OperationType.Add);

        return await result.FillAsync(async () =>
        {
            TKey newId = model.Id;
            var added = await GetCurrentDatabase().HashSetAsync(rKey
                , new RedisValue(newId.ToString())
                , new RedisValue(JsonSerializer.Serialize(model)));

            if (expireTime > 0)
            {
                await GetCurrentDatabase().ExecuteAsync($"HEXPIRE {key} {expireTime * 60} FIELDS 1 {newId.ToString()}");
            }

            if (added)
                result.Set(StatusCode.Succeeded, newId);
            else
                result.Set(StatusCode.Failed);
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
                var newValue = mapper.Map<TEditableVM, TModel>(model);

                var value = await GetCurrentDatabase()
                    .HashSetAsync(rKey
                        , idValue
                        , new RedisValue(JsonSerializer.Serialize(newValue)));

                result.Set(StatusCode.Succeeded, value);
            }
            else
            {
                result.Set(StatusCode.NotExists);
            }
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
        });
    }

    public virtual async Task<IResponse<GridVM<TKey, SelectOptionVM<TKey, object>>>> GetDictionary(GridSearchModelVM<TKey, TSearchVM> model = null)
    {
        var result = new Response<GridVM<TKey, SelectOptionVM<TKey, object>>>(GetType().Name, OperationType.Fetch);

        return await result.FillAsync(async () =>
        {
            var resultValue = new GridVM<TKey, SelectOptionVM<TKey, object>>(model);
            var items = await InternalGetAll();

            // TODO: Needs test
            var list = Search(items, model.Fields)
                .Select(x => mapper.Map<SelectOptionVM<TKey, object>>(x));

            resultValue.List = list.ToList();

            result.Set(StatusCode.Succeeded, resultValue!);
        });
    }
}