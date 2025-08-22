// Licensed under the MIT license.

using Sardanapal.Contract.IModel;

namespace Sardanapal.Contract.IRepository;

public interface IMemoryRepository<TKey, TModel> : ICrudRepository<TKey, TModel>
    where TKey : IComparable<TKey>, IEquatable<TKey>
    where TModel : class, IBaseEntityModel<TKey>, new()
{
    IEnumerable<TModel> FetchAll(CancellationToken ct = default);
    Task<IEnumerable<TModel>> FetchAllAsync(CancellationToken ct = default);
}
