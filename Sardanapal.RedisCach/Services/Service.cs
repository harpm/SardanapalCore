using System.Text.Json;
using StackExchange.Redis;
using Sardanapal.DomainModel.Domain;
using Sardanapal.ViewModel.Response;
using Sardanapal.ViewModel.Models;
using AutoMapper;

namespace Sardanapal.RedisCache.Services;

public abstract class CacheService<TModel, TSearchVM, TVM, TNewVM, TEditableVM>
    : ICacheService<TSearchVM, TVM, TNewVM, TEditableVM>
    where TSearchVM : class, new()
    where TVM : class, new()
    where TNewVM : class, new()
    where TEditableVM : class, new()
    where TModel : IBaseEntityModel<Guid>, new()
{
    protected IConnectionMultiplexer ConnMultiplexer { get; set; }
    protected IMapper Mapper { get; set; }
    protected int expireTime;

    protected abstract string Key { get; }

    public CacheService(IConnectionMultiplexer connectionMultiplexer, IMapper mapper, int expireTime = 0)
    {
        this.expireTime = expireTime;
        this.ConnMultiplexer = connectionMultiplexer;
        this.Mapper = mapper;
    }

    protected virtual IDatabase GetCurrentDatabase()
    {
        return ConnMultiplexer.GetDatabase();
    }

    public virtual async Task<IResponse<TVM>> Get(Guid Id)
    {
        var result = new Response<TVM>(this.GetType().Name, OperationType.Fetch);

        try
        {
            var rKey = new RedisKey(Key);

            string item = await GetCurrentDatabase().HashGetAsync(rKey, new RedisValue(Id.ToString()));

            if (!string.IsNullOrWhiteSpace(item))
            {
                var model = JsonSerializer.Deserialize<TModel>(item);
                var value = Mapper.Map<TVM>(model);
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
        var result = new Response<GridVM<T, TSearchVM>>(this.GetType().Name, OperationType.Fetch);

        return await result.Create(async () =>
        {
            var resultValue = new GridVM<T, TSearchVM>(model);
            var items = await GetCurrentDatabase().HashGetAllAsync(new RedisKey(Key));

            if (items != null && items.Length > 0)
            {
                var list = items
                    .Select(x => Mapper.Map<T>(JsonSerializer.Deserialize<TModel>(x.Value)))
                    .AsEnumerable();

                // TODO: Search with the model

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

    public virtual async Task<IResponse<Guid>> Add(TNewVM Model)
    {
        var result = new Response<Guid>(this.GetType().Name, OperationType.Add);

        return await result.Create(async () =>
        {
            var newId = Guid.NewGuid();
            var rKey = new RedisKey(Key);
            var added = await GetCurrentDatabase().HashSetAsync(rKey
                , new RedisValue(newId.ToString())
                , new RedisValue(JsonSerializer.Serialize(Model)));

            bool setExpiration = true;

            if (this.expireTime > 0)
            {
                setExpiration = await GetCurrentDatabase()
                    .KeyExpireAsync(rKey, DateTime.UtcNow.AddMinutes(this.expireTime));
            }

            if (added && setExpiration)
                result.Set(StatusCode.Succeeded, newId);
            else
                result.Set(StatusCode.Failed);

            return result;
        });
    }

    public virtual async Task<IResponse<bool>> Edit(Guid Id, TEditableVM Model)
    {
        var result = new Response<bool>(this.GetType().Name, OperationType.Edit);

        return await result.Create(async () =>
        {
            var old = await Get(Id);
            if (old.StatusCode == StatusCode.Succeeded)
            {
                var value = await GetCurrentDatabase()
                    .HashSetAsync(new RedisKey(Key)
                        , new RedisValue(Id.ToString())
                        , new RedisValue(JsonSerializer.Serialize(Model)));

                result.Set(StatusCode.Succeeded, value);
            }
            else
            {
                result.Set(StatusCode.NotExists);
            }

            return result;
        });
    }

    public virtual async Task<IResponse<bool>> Delete(Guid Id)
    {
        var result = new Response<bool>(this.GetType().Name, OperationType.Delete);

        return await result.Create(async () =>
        {
            var old = await Get(Id);
            if (old.StatusCode == StatusCode.Succeeded)
            {
                var value = await GetCurrentDatabase()
                    .HashDeleteAsync(new RedisKey(Key), new RedisValue(Id.ToString()));
                result.Set(StatusCode.Succeeded, value);
            }
            else
            {
                result.Set(StatusCode.NotExists);
            }

            return result;
        });
    }
}