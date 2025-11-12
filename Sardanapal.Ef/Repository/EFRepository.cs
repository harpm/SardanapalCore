
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Sardanapal.Contract.IModel;
using Sardanapal.Contract.IRepository;

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

    public virtual TKey Add(TModel model, CancellationToken ct = default)
    {
        EnsureNotNullReference(model);

        _unitOfWork.Add(model);
        _unitOfWork.SaveChanges();
        return model.Id;
    }

    public virtual async Task<TKey> AddAsync(TModel model, CancellationToken ct = default)
    {
        EnsureNotNullReference(model);

        await _unitOfWork.AddAsync(model);
        await _unitOfWork.SaveChangesAsync();
        return model.Id;
    }

    public IQueryable<TModel> FetchAll(CancellationToken ct = default)
    {
        return GetInternalQuery();
    }

    public async Task<IQueryable<TModel>> FetchAllAsync(CancellationToken ct = default)
    {
        return GetInternalQuery();
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

    public bool Delete(TKey key, CancellationToken ct = default)
    {
        EnsureNotNullReference(key);
        var deletingEntry = this.FetchById(key);
        var res = _unitOfWork.Set<TModel>().Remove(deletingEntry);
        return res.State == EntityState.Deleted;
    }

    public async Task<bool> DeleteAsync(TKey key, CancellationToken ct = default)
    {
        EnsureNotNullReference(key);
        var deletingEntry = await this.FetchByIdAsync(key);
        var res = _unitOfWork.Set<TModel>().Remove(deletingEntry);
        return res.State == EntityState.Deleted;
    }


    public void DeleteRange(IEnumerable<TKey> keys, CancellationToken ct = default)
    {
        EnsureNotNullCollection(keys);

        var deletingEntries = this.FetchAll().AsQueryable().Where(x => keys.Contains(x.Id)).ToArray();
        _unitOfWork.Set<TModel>().RemoveRange(deletingEntries);
    }

    public async Task DeleteRangeAsync(IEnumerable<TKey> keys, CancellationToken ct = default)
    {
        EnsureNotNullCollection(keys);

        var deletingEntries = await this.FetchAll().AsQueryable().Where(x => keys.Contains(x.Id)).ToArrayAsync();
        _unitOfWork.Set<TModel>().RemoveRange(deletingEntries);
    }


    protected void EnsureNotNullReference<T>(T values, CancellationToken ct = default)
    {
        if (values == null) throw new ArgumentNullException(nameof(values));
    }

    protected void EnsureNotNullCollection<T>(IEnumerable<T> values, CancellationToken ct = default)
    {
        if (values == null || values.Count() == 0) throw new ArgumentNullException(nameof(values));
    }

    public IDbContextTransaction BeginTransaction()
    {
        return _unitOfWork.Database.BeginTransaction();
    }

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
    {
        return _unitOfWork.Database.BeginTransactionAsync(ct);
    }

    public bool SaveChanges(CancellationToken ct = default)
    {
        return _unitOfWork.SaveChanges() > 0;
    }   

    public async Task<bool> SaveChangesAsync(CancellationToken ct = default)
    {
        var res = await _unitOfWork.SaveChangesAsync();
        return res > 0;
    }
}
