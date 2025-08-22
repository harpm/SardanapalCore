
using Microsoft.EntityFrameworkCore.Storage;
using Sardanapal.Contract.IModel;

namespace Sardanapal.Contract.IRepository;

public interface IEFRepository<TKey, TModel> : ICrudRepository<TKey, TModel>
    where TKey : IComparable<TKey>, IEquatable<TKey>
    where TModel : class, IBaseEntityModel<TKey>, new()
{
    IQueryable<TModel> FetchAll(CancellationToken ct = default);
    Task<IQueryable<TModel>> FetchAllAsync(CancellationToken ct = default);

    IDbContextTransaction BeginTransaction();
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);

    bool SaveChanges(CancellationToken ct = default);
    Task<bool> SaveChangesAsync(CancellationToken ct = default);
}
