namespace Sardanapal.RedisCach.Services;

public interface ICachService<TKey, TModel>
    where TKey : IEquatable<TKey>, IComparable<TKey>
    where TModel : new()
{
    Task<TModel> Get(TKey Id);
    Task<IEnumerable<TModel>> GetAll();
    Task<TKey> Add(TModel Model);
    Task<bool> Edit(TKey Id, TModel Model);
    Task<bool> Delete(TKey Id);
}