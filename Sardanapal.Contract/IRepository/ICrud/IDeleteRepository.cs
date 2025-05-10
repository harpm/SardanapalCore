
namespace Sardanapal.Contract.IRepository.ICrud;

public interface IDeleteRepository<TKey>
    where TKey : IComparable<TKey>, IEquatable<TKey>
{
    bool Delete(TKey key);
    Task<bool> DeleteAsync(TKey key);
}
