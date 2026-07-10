
namespace Sardanapal.Contract.IModel;

public interface ICacheModel<TKey> : IBaseEntityModel<TKey>
    where TKey : IEquatable<TKey>, IComparable<TKey>
{

}
