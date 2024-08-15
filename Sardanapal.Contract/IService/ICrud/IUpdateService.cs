
using Sardanapal.ViewModel.Response;

namespace Sardanapal.Interface.IService.ICrud;

public interface IUpdateService<TKey, TEditableVM>
    where TKey : IEquatable<TKey>, IComparable<TKey>
    where TEditableVM : class, new()
{
    Task<IResponse<TEditableVM>> GetEditable(TKey Id);
    Task<IResponse<bool>> Edit(TKey Id, TEditableVM Model);
}
