using Sardanapal.ViewModel.Response;

namespace Sardanapal.RedisCache.Services;

public interface ICacheService<TKey, TModel>
    where TKey : IEquatable<TKey>, IComparable<TKey>
    where TModel : new()
{
    Task<IResponse<TModel>> Get(TKey Id);
    Task<IResponse<IEnumerable<TModel>>> GetAll();
    Task<IResponse<TKey>> Add(TModel Model);
    Task<IResponse<bool>> Edit(TKey Id, TModel Model);
    Task<IResponse<bool>> Delete(TKey Id);
}