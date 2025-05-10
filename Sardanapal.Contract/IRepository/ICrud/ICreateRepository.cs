
using Sardanapal.Contract.IModel;

namespace Sardanapal.Contract.IRepository.ICrud;

public interface ICreateRepository<TKey, TModel>
    where TKey : IComparable<TKey>, IEquatable<TKey>
    where TModel : class, IDomainModel, new()
{
    /// <summary>
    /// Adds new model in the parameters accordingly into the database
    /// </summary>
    /// <param name="model">Given model will be added to the database</param>
    /// <returns>The Id of the new added entry</returns>
    TKey Add(TModel model);


    /// <summary>
    /// Adds new model in the parameters accordingly into the database
    /// </summary>
    /// <param name="model">Given model will be added to the database</param>
    /// <returns>The Id of the new added entry</returns>
    Task<TKey> AddAsync(TModel model);
}