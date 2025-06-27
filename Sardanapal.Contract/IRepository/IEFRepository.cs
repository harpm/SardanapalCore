
using Microsoft.EntityFrameworkCore.Storage;
using Sardanapal.Contract.IModel;

namespace Sardanapal.Contract.IRepository;

public interface IEFRepository<TKey, TModel> : ICrudRepository<TKey, TModel>
    where TKey : IComparable<TKey>, IEquatable<TKey>
    where TModel : class, IBaseEntityModel<TKey>, new()
{
    IDbContextTransaction BeginTransaction();
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);

    bool SaveChanges(CancellationToken ct = default);
    Task<bool> SaveChangesAsync(CancellationToken ct = default);
}