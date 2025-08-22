// Licensed under the MIT license.

using System.Collections.Concurrent;
using Sardanapal.Contract.IModel;
using Sardanapal.Contract.IRepository;

namespace Sardanapal.Service.Repository;

public abstract class MemoryRepositoryBase<TKey, TModel> : IMemoryRepository<TKey, TModel>
    where TKey : IComparable<TKey>, IEquatable<TKey>
    where TModel : class, IBaseEntityModel<TKey>, new()
{
    protected virtual ConcurrentDictionary<TKey, TModel> _db { get; set; }
        = new ConcurrentDictionary<TKey, TModel>();

    public virtual TKey Add(TModel model, CancellationToken ct = default)
    {
        var res = _db.TryAdd(model.Id, model);
        return res ? model.Id : default;
    }

    public virtual Task<TKey> AddAsync(TModel model, CancellationToken ct = default)
    {
        var res = _db.TryAdd(model.Id, model);
        return Task.FromResult<TKey>(res ? model.Id : default);
    }
    public virtual IEnumerable<TModel> FetchAll(CancellationToken ct = default)
    {
        return _db.Select(kvp => kvp.Value).AsEnumerable();
    }

    public virtual Task<IEnumerable<TModel>> FetchAllAsync(CancellationToken ct = default)
    {
        return Task.FromResult(_db.Select(kvp => kvp.Value).ToList().AsEnumerable());
    }

    public virtual TModel FetchById(TKey id, CancellationToken ct = default)
    {
        return _db.TryGetValue(id, out TModel model) ? model : default;
    }

    public virtual Task<TModel> FetchByIdAsync(TKey id, CancellationToken ct = default)
    {
        return Task.FromResult(_db.TryGetValue(id, out TModel model) ? model : default);
    }
    public virtual bool Update(TKey key, TModel model, CancellationToken ct = default)
    {
        if (_db.TryGetValue(key, out TModel oldModel))
        {
            return _db.TryUpdate(key, model, oldModel);
        }
        else return false;
    }

    public virtual Task<bool> UpdateAsync(TKey key, TModel model, CancellationToken ct = default)
    {

        if (_db.TryGetValue(key, out TModel oldModel))
        {
            return Task.FromResult(_db.TryUpdate(key, model, oldModel));
        }
        else return Task.FromResult(false);
    }
    public virtual bool Delete(TKey key, CancellationToken ct = default)
    {
        return _db.TryRemove(key, out TModel _);
    }


    public virtual Task<bool> DeleteAsync(TKey key, CancellationToken ct = default)
    {
        return Task.FromResult(_db.TryRemove(key, out TModel _));
    }

    public virtual void DeleteRange(IEnumerable<TKey> keys, CancellationToken ct = default)
    {
        foreach (var key in keys)
        {
            _db.Remove(key, out TModel _);
        }
    }

    public virtual Task DeleteRangeAsync(IEnumerable<TKey> keys, CancellationToken ct = default)
    {
        return Task.Run(() =>
        {
            foreach (var key in keys)
            {
                _db.Remove(key, out TModel _);
            }
        });
    }
}
