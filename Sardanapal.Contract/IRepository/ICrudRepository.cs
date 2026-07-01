
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
    /// <summary>
    /// Returns a composable query over the entity set. The query is deferred and
    /// executed only when enumerated, so the underlying <c>DbContext</c> must
    /// remain alive and in scope when the query is materialized. The DbContext,
    /// repository, and consuming service should share the same DI scope
    /// (typically Scoped) so the context is available during enumeration.
    /// </summary>
    IQueryable<TModel> FetchAll(CancellationToken ct = default);

    /// <summary>
    /// Asynchronously returns a composable query over the entity set. The query
    /// is deferred and executed only when enumerated, so the underlying
    /// <c>DbContext</c> must remain alive and in scope when the query is
    /// materialized. The DbContext, repository, and consuming service should
    /// share the same DI scope (typically Scoped).
    /// </summary>
    Task<IQueryable<TModel>> FetchAllAsync(CancellationToken ct = default);
}


public interface ICrudRepository<TKey, TModel> : IReadRepository<TKey, TModel>
    , ICreateRepository<TKey, TModel>
    , IUpdateRepository<TKey, TModel>
    , IDeleteRepository<TKey>
    where TKey : IComparable<TKey>, IEquatable<TKey>
    where TModel : class, IDomainModel, new()
{

}
