
using Sardanapal.Contract.IModel;

namespace Sardanapal.Contract.IRepository;

public interface IEFRepository<TKey, TModel> : ICrudRepository<TKey, TModel>
    where TKey : IComparable<TKey>, IEquatable<TKey>
    where TModel : class, IBaseEntityModel<TKey>, new()
{
    bool SaveChanges(CancellationToken ct = default);
    Task<bool> SaveChangesAsync(CancellationToken ct = default);
}