
namespace Sardanapal.Contract.IModel;

public interface ITreeableEntityModel<TKey> : IBaseEntityModel<TKey>, IDomainModel
    where TKey : IComparable<TKey>, IEquatable<TKey>
{
    TKey? ParentId { get; set; }
    ITreeableEntityModel<TKey>? Parent { get; set; }
    ICollection<ITreeableEntityModel<TKey>> Children { get; set; }
}
