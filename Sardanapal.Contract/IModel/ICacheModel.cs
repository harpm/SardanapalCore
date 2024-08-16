
namespace Sardanapal.Contract.IModel;

public interface ICachModel<TKey> : IBaseEntityModel<TKey>
    where TKey : IEquatable<TKey>, IComparable<TKey>
{

}
