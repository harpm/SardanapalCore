
using Sardanapal.Contract.IModel;

namespace Sardanapal.Contract.IRepository.ICrud;

public interface ICreateRepository<TKey, TModel>
    where TKey : IComparable<TKey>, IEquatable<TKey>
    where TModel : class, IDomainModel, new()
{
    /// <summary>
    /// Adds new model in the parameters accordingly into the database
    /// </summary>
    /// <param name="model">Given model will be added to the database; its Id is populated after the change is persisted</param>
    void Add(TModel model, CancellationToken ct = default);


    /// <summary>
    /// Adds new model in the parameters accordingly into the database
    /// </summary>
    /// <param name="model">Given model will be added to the database; its Id is populated after the change is persisted</param>
    Task AddAsync(TModel model, CancellationToken ct = default);
}