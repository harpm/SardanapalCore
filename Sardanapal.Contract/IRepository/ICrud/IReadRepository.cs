
using Sardanapal.Contract.IModel;

namespace Sardanapal.Contract.IRepository.ICrud;

public interface IReadRepository<TKey, TModel>
    where TKey : IComparable<TKey>, IEquatable<TKey>
    where TModel : class, IDomainModel, new()
{
    TModel FetchById(TKey id);
    Task<TModel> FetchByIdAsync(TKey id);
    IEnumerable<TModel> FetchAll();
    Task<IEnumerable<TModel>> FetchAllAsync();
}
