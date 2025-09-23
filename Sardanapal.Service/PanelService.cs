
using AutoMapper;
using Microsoft.Extensions.Logging;
using Sardanapal.Contract.IModel;
using Sardanapal.Contract.IRepository;
using Sardanapal.Contract.IService;
using Sardanapal.ViewModel.Models;
using Sardanapal.ViewModel.Response;

namespace Sardanapal.Service;

public abstract class PanelServiceBase<TRepository, TKey, TEntity, TSearchVM, TVM, TNewVM, TEditableVM>
    : CrudServiceBase<TRepository, TKey, TEntity, TSearchVM, TVM, TNewVM, TEditableVM>
    , IPanelService<TKey, TSearchVM, TVM, TNewVM, TEditableVM>
    where TRepository : ICrudRepository<TKey, TEntity>
    where TKey : IComparable<TKey>, IEquatable<TKey>
    where TEntity : class, IBaseEntityModel<TKey>, new()
    where TSearchVM : class, new()
    where TVM : class, new()
    where TNewVM : class, new()
    where TEditableVM : class, new()
{
    protected PanelServiceBase(TRepository repository, IMapper mapper, ILogger logger)
        : base(repository, mapper, logger)
    {
        
    }

    public abstract Task<IResponse<GridVM<TKey, SelectOptionVM<TKey, object>>>>
        GetDictionary(GridSearchModelVM<TKey, TSearchVM> SearchModel = null, CancellationToken ct = default);
}
