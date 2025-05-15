
using Sardanapal.Contract.IModel;
using Sardanapal.Contract.IRepository;

namespace Sardanapal.Ef.Repository;

public interface IEFRepository<TKey, TModel> : ICrudRepository<TKey, TModel>
    where TKey : IComparable<TKey>, IEquatable<TKey>
    where TModel : class, IBaseEntityModel<TKey>, new()
{
    bool SaveChanges();
    Task<bool> SaveChangesAsync();
}