
using Sardanapal.Domain.Model;

namespace Sardanapal.RedisCach.Models;

public interface ICachModel<TKey> : IBaseEntityModel<TKey>
    where TKey : IEquatable<TKey>, IComparable<TKey>
{

}
