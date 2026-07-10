
using Sardanapal.Contract.IModel;

namespace Sardanapal.Contract.IRepository.ICrud;

public interface IUpdateRepository<TKey, TModel>
    where TKey : IComparable<TKey>, IEquatable<TKey>
    where TModel : class, IDomainModel, new()
{
    bool Update(TKey key, TModel model, CancellationToken ct = default);
    Task<bool> UpdateAsync(TKey key, TModel model, CancellationToken ct = default);
}
