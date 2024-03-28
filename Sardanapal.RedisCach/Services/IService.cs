using Sardanapal.ViewModel.Response;

namespace Sardanapal.RedisCache.Services;

public interface ICacheService<TModel, TNewVM, TEditableVM>
    where TModel : new()
    where TNewVM : class
    where TEditableVM : class
{
    Task<IResponse<TModel>> Get(Guid Id);
    Task<IResponse<IEnumerable<TModel>>> GetAll();
    Task<IResponse<Guid>> Add(TNewVM Model);
    Task<IResponse<bool>> Edit(Guid Id, TEditableVM Model);
    Task<IResponse<bool>> Delete(Guid Id);
}