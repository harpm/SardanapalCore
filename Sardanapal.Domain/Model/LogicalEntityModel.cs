
using Sardanapal.Contract.IModel;

namespace Sardanapal.Domain.Model;

public abstract class LogicalBaseEntityModel<TKey> : BaseEntityModel<TKey>, ILogicalEntityModel
    where TKey : IComparable<TKey>, IEquatable<TKey>
{
    public virtual bool IsDeleted { get; set; }
}

public abstract class LogicalEntityModel<TKey, TUserKey> : EntityModel<TKey, TUserKey>, ILogicalEntityModel
    where TUserKey : IComparable<TUserKey>, IEquatable<TUserKey>
    where TKey : IComparable<TKey>, IEquatable<TKey>
{
    public virtual bool IsDeleted { get; set; }
}