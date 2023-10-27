using Sardanapal.RedisCache.Models;

namespace Sardanapal.RedisCache.Services;

public interface ICacheService<TKey, TModel>
    where TKey : IEquatable<TKey>, IComparable<TKey>
    where TModel : new()
{
    Task<CacheResponse<TModel>> Get(TKey Id);
    Task<CacheResponse<IEnumerable<TModel>>> GetAll();
    Task<CacheResponse<TKey>> Add(TModel Model);
    Task<CacheResponse<bool>> Edit(TKey Id, TModel Model);
    Task<CacheResponse<bool>> Delete(TKey Id);
}