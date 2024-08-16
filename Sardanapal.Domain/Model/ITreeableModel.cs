
using Sardanapal.Contract.IModel;

namespace Sardanapal.Domain.Model;

public abstract class TreeableEntityModel<TKey> : ITreeableEntityModel<TKey>
    where TKey : IComparable<TKey>, IEquatable<TKey>
{
    public TKey Id { get; set; }
    public TKey? ParentId { get; set; }
    public ITreeableEntityModel<TKey>? Parent { get; set; }
    public ICollection<ITreeableEntityModel<TKey>> Children { get; set; }
}