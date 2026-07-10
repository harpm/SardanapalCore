
namespace Sardanapal.Contract.IModel;

public interface IEntityModel<TKey, TUserKey> : IBaseEntityModel<TKey>, IDomainModel
    where TUserKey : IComparable<TUserKey>, IEquatable<TUserKey>
    where TKey : IComparable<TKey>, IEquatable<TKey>
{
    TUserKey CreateBy { get; set; }
    TUserKey ModifiedBy { get; set; }
    DateTime CreatedOnUtc { get; set; }
    DateTime ModifiedOnUtc { get; set; }
}