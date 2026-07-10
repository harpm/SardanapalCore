
using Sardanapal.Contract.IModel;

namespace Sardanapal.Domain.Model;

public abstract class EntityModel<TKey, TUserKey> : BaseEntityModel<TKey>, IEntityModel<TKey, TUserKey>
    where TKey : IComparable<TKey>, IEquatable<TKey>
    where TUserKey : IComparable<TUserKey>, IEquatable<TUserKey>
{
    public virtual TUserKey CreateBy { get; set; }
    public virtual TUserKey ModifiedBy { get; set; }
    public virtual DateTime CreatedOnUtc { get; set; }
    public virtual DateTime ModifiedOnUtc { get; set; }
}