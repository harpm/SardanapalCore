
using Sardanapal.ViewModel.Response;

namespace Sardanapal.Interface.IService.ICrud;

public interface ICreateService<TKey, TNewVM>
    where TKey : IEquatable<TKey>, IComparable<TKey>
    where TNewVM : new()
{
    Task<IResponse<TKey>> Add(TNewVM Model);
}
