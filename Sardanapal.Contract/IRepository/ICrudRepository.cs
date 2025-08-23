
using Microsoft.EntityFrameworkCore.Storage;
using Sardanapal.Contract.IModel;
using Sardanapal.Contract.IRepository.ICrud;

namespace Sardanapal.Contract.IRepository;

public interface IEFCrudRepository<TKey, TModel> : IReadRepository<TKey, TModel>
    , ICreateRepository<TKey, TModel>
    , IUpdateRepository<TKey, TModel>
    , IDeleteRepository<TKey>
    where TKey : IComparable<TKey>, IEquatable<TKey>
    where TModel : class, IDomainModel, new()
{
    IQueryable<TModel> FetchAll(CancellationToken ct = default);
    Task<IQueryable<TModel>> FetchAllAsync(CancellationToken ct = default);

    IDbContextTransaction BeginTransaction();
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);

    bool SaveChanges(CancellationToken ct = default);
    Task<bool> SaveChangesAsync(CancellationToken ct = default);
}


public interface ICrudRepository<TKey, TModel> : IReadRepository<TKey, TModel>
    , ICreateRepository<TKey, TModel>
    , IUpdateRepository<TKey, TModel>
    , IDeleteRepository<TKey>
    where TKey : IComparable<TKey>, IEquatable<TKey>
    where TModel : class, IDomainModel, new()
{
    IEnumerable<TModel> FetchAll(CancellationToken ct = default);
    Task<IEnumerable<TModel>> FetchAllAsync(CancellationToken ct = default);
}
