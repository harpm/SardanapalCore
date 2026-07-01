
using Microsoft.EntityFrameworkCore;
using Sardanapal.Contract.IModel;
using Sardanapal.Contract.IRepository;
using Sardanapal.Localization;

namespace Sardanapal.Ef.Repository;

public abstract class EFRepositoryBase<TContext, TKey, TModel> : IEFCrudRepository<TKey, TModel>
    where TContext : DbContext
    where TKey : IComparable<TKey>, IEquatable<TKey>
    where TModel : class, IBaseEntityModel<TKey>, new()
{
    protected readonly TContext _unitOfWork;

    protected EFRepositoryBase(TContext context)
    {
        this._unitOfWork = context;
    }

    protected virtual IQueryable<TModel> GetInternalQuery()
    {
        return _unitOfWork.Set<TModel>();
    }

    public virtual void Add(TModel model, CancellationToken ct = default)
    {
        EnsureNotNullReference(model);

        _unitOfWork.Add(model);
    }

    public virtual async Task AddAsync(TModel model, CancellationToken ct = default)
    {
        EnsureNotNullReference(model);

        await _unitOfWork.AddAsync(model, ct);
    }

    /// <summary>
    /// Returns a deferred query bound to this repository's <c>DbContext</c>.
    /// Enumerate (e.g. via <c>ToListAsync</c>) while the context is still in scope.
    /// </summary>
    public IQueryable<TModel> FetchAll(CancellationToken ct = default)
    {
        return GetInternalQuery();
    }

    /// <summary>
    /// Returns a deferred query bound to this repository's <c>DbContext</c>.
    /// Enumerate (e.g. via <c>ToListAsync</c>) while the context is still in scope.
    /// </summary>
    public Task<IQueryable<TModel>> FetchAllAsync(CancellationToken ct = default)
    {
        return Task.FromResult(GetInternalQuery());
    }

    public TModel FetchById(TKey id, CancellationToken ct = default)
    {
        EnsureNotNullReference(id);
        return GetInternalQuery().Where(x => x.Id.Equals(id)).FirstOrDefault();
    }

    public async Task<TModel> FetchByIdAsync(TKey id, CancellationToken ct = default)
    {
        EnsureNotNullReference(id);
        return await GetInternalQuery().Where(x => x.Id.Equals(id)).FirstOrDefaultAsync();
    }

    public bool Update(TKey key, TModel model, CancellationToken ct = default)
    {
        EnsureNotNullReference(key);
        EnsureNotNullReference(model);

        var res = _unitOfWork.Update(model);
        return res.State == EntityState.Modified;
    }

    public Task<bool> UpdateAsync(TKey key, TModel model, CancellationToken ct = default)
    {
        EnsureNotNullReference(key);
        EnsureNotNullReference(model);

        var res = _unitOfWork.Update(model);
        return Task.FromResult(res.State == EntityState.Modified);
    }

    public void Delete(TKey key, CancellationToken ct = default)
    {
        EnsureNotNullReference(key);
        var deletingEntry = this.FetchById(key);
        if (deletingEntry == null) throw new KeyNotFoundException(ResourceHelper.CreateNotFoundByKeyMessage(key));
        _unitOfWork.Set<TModel>().Remove(deletingEntry);
    }

    public async Task DeleteAsync(TKey key, CancellationToken ct = default)
    {
        EnsureNotNullReference(key);
        var deletingEntry = await this.FetchByIdAsync(key);
        if (deletingEntry == null) throw new KeyNotFoundException(ResourceHelper.CreateNotFoundByKeyMessage(key));
        _unitOfWork.Set<TModel>().Remove(deletingEntry);
    }

    public void DeleteRange(IEnumerable<TKey> keys, CancellationToken ct = default)
    {
        EnsureNotNullCollection(keys);

        var query = _unitOfWork.Set<TModel>()
            .Where(x => keys.Contains(x.Id));

        if (typeof(ILogicalEntityModel).IsAssignableFrom(typeof(TModel)))
            SoftDelete(query);
        else
            query.ExecuteDelete();
    }

    public Task DeleteRangeAsync(IEnumerable<TKey> keys, CancellationToken ct = default)
    {
        EnsureNotNullCollection(keys);

        var query = _unitOfWork.Set<TModel>()
            .Where(x => keys.Contains(x.Id));

        if (typeof(ILogicalEntityModel).IsAssignableFrom(typeof(TModel)))
            return SoftDeleteAsync(query);
        return query.ExecuteDeleteAsync(ct);
    }

    private void SoftDelete(IQueryable<TModel> query)
    {
        query.ExecuteUpdate(s => s.SetProperty(
            x => ((ILogicalEntityModel)x).IsDeleted, true));
    }

    private Task SoftDeleteAsync(IQueryable<TModel> query)
    {
        return query.ExecuteUpdateAsync(s => s.SetProperty(
            x => ((ILogicalEntityModel)x).IsDeleted, true));
    }

    protected void EnsureNotNullReference<T>(T values, CancellationToken ct = default)
    {
        if (values == null) throw new ArgumentNullException(nameof(values));
    }

    protected void EnsureNotNullCollection<T>(IEnumerable<T> values, CancellationToken ct = default)
    {
        if (values == null || values.Count() == 0) throw new ArgumentNullException(nameof(values));
    }
}
