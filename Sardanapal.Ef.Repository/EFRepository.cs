
using Microsoft.EntityFrameworkCore;
using Sardanapal.Contract.IModel;

namespace Sardanapal.Ef.Repository;

public abstract class EFRepositoryBase<TContext, TKey, TModel> : IEFRepository<TKey, TModel>
    where TContext : DbContext
    where TKey : IComparable<TKey>, IEquatable<TKey>
    where TModel : class, IBaseEntityModel<TKey>, new()
{
    protected readonly TContext _unitOfWork;

    protected EFRepositoryBase(TContext context)
    {
        this._unitOfWork = context;
    }

    public virtual TKey Add(TModel model)
    {
        _unitOfWork.Add(model);
        _unitOfWork.SaveChanges();
        return model.Id;
    }

    public virtual async Task<TKey> AddAsync(TModel model)
    {
        await _unitOfWork.AddAsync(model);
        await _unitOfWork.SaveChangesAsync();
        return model.Id;
    }

    public IEnumerable<TModel> FetchAll()
    {
        return _unitOfWork.Set<TModel>();
    }

    public async Task<IEnumerable<TModel>> FetchAllAsync()
    {
        return _unitOfWork.Set<TModel>();
    }

    public TModel FetchById(TKey id)
    {
        return _unitOfWork.Set<TModel>().Where(x => x.Id.Equals(id)).FirstOrDefault();
    }

    public async Task<TModel> FetchByIdAsync(TKey id)
    {
        return await _unitOfWork.Set<TModel>().Where(x => x.Id.Equals(id)).FirstOrDefaultAsync();
    }

    public bool Update(TKey key, TModel model)
    {
        var res = _unitOfWork.Update(model);
        return res.State == EntityState.Modified;
    }

    public Task<bool> UpdateAsync(TKey key, TModel model)
    {
        var res = _unitOfWork.Update(model);
        return Task.FromResult(res.State == EntityState.Modified);
    }

    public bool Delete(TKey key)
    {
        var deletingEntry = this.FetchById(key);
        var res = _unitOfWork.Set<TModel>().Remove(deletingEntry);
        return res.State == EntityState.Deleted;
    }

    public async Task<bool> DeleteAsync(TKey key)
    {
        var deletingEntry = await this.FetchByIdAsync(key);
        var res = _unitOfWork.Set<TModel>().Remove(deletingEntry);
        return res.State == EntityState.Deleted;
    }

    public bool SaveChanges()
    {
        return _unitOfWork.SaveChanges() > 0;
    }   

    public async Task<bool> SaveChangesAsync()
    {
        var res = await _unitOfWork.SaveChangesAsync();
        return res > 0;
    }
}