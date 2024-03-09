
namespace Sardanapal.DomainModel.Domain;

public interface ITreeableEntityModel<TKey> : IBaseEntityModel<TKey>
    where TKey : IComparable<TKey>, IEquatable<TKey>
{
    TKey? ParentId { get; set; }
    ITreeableEntityModel<TKey>? Parent { get; set; }
    ICollection<ITreeableEntityModel<TKey>> Children { get; set; }
}

public abstract class TreeableEntityModel<TKey> : ITreeableEntityModel<TKey>
    where TKey : IComparable<TKey>, IEquatable<TKey>
{
    public TKey Id { get; set; }
    public TKey? ParentId { get; set; }
    public ITreeableEntityModel<TKey>? Parent { get; set; }
    public ICollection<ITreeableEntityModel<TKey>> Children { get; set; }
}