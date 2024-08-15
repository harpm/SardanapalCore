

namespace Sardanapal.Contract.IModel;

public interface IBaseEntityModel<TKey> : IDomainModel
    where TKey : IComparable<TKey>, IEquatable<TKey>
{
    TKey Id { get; set; }
}