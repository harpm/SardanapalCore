
using Sardanapal.Contract.IModel;
using Sardanapal.Contract.IRepository.ICrud;

namespace Sardanapal.Contract.IRepository;

public interface ICrudRepository<TKey, TModel> : IReadRepository<TKey, TModel>
    , ICreateRepository<TKey, TModel>
    , IUpdateRepository<TKey, TModel>
    , IDeleteRepository<TKey>
    where TKey : IComparable<TKey>, IEquatable<TKey>
    where TModel : class, IDomainModel, new()
{
}
