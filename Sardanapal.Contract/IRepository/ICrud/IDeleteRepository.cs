
namespace Sardanapal.Contract.IRepository.ICrud;

public interface IDeleteRepository<TKey>
    where TKey : IComparable<TKey>, IEquatable<TKey>
{
    bool Delete(TKey key, CancellationToken ct = default);
    Task<bool> DeleteAsync(TKey key, CancellationToken ct = default);

    void DeleteRange(IEnumerable<TKey> keys, CancellationToken ct = default);
    Task DeleteRangeAsync(IEnumerable<TKey> keys, CancellationToken ct = default);
}
