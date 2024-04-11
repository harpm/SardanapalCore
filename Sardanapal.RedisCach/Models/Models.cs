
namespace Sardanapal.RedisCach.Models;

public interface ICachModels<TKey>
    where TKey : IEquatable<TKey>, IComparable<TKey>
{
    TKey Id { get; set; }
}
