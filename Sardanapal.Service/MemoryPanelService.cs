
using AutoMapper;
using Microsoft.Extensions.Logging;
using Sardanapal.Contract.IModel;
using Sardanapal.Contract.IRepository;
using Sardanapal.Contract.IService;
using Sardanapal.Service.Utilities;
using Sardanapal.ViewModel.Models;
using Sardanapal.ViewModel.Response;

namespace Sardanapal.Service;

public abstract class MemoryPanelServiceBase<TRepository, TKey, TEntity, TSearchVM, TVM, TNewVM, TEditableVM>
    : MemoryCrudServiceBase<TRepository, TKey, TEntity, TSearchVM, TVM, TNewVM, TEditableVM>
    , IPanelService<TKey, TSearchVM, TVM, TNewVM, TEditableVM>
    where TRepository : class, ICrudRepository<TKey, TEntity>, IMemoryRepository<TKey, TEntity>
    where TKey : IComparable<TKey>, IEquatable<TKey>
    where TEntity : class, IBaseEntityModel<TKey>, new()
    where TSearchVM : class, new()
    where TVM : class, new()
    where TNewVM : class, new()
    where TEditableVM : class, new()
{
    protected MemoryPanelServiceBase(TRepository repository, IMapper mapper, ILogger logger)
        : base(repository, mapper, logger)
    {
        
    }

    public virtual async Task<IResponse<GridVM<TKey, SelectOptionVM<TKey, object>>>>
            GetDictionary(GridSearchModelVM<TKey, TSearchVM> searchModel = null, CancellationToken ct = default)
    {
        IResponse<GridVM<TKey, SelectOptionVM<TKey, object>>> result
            = new Response<GridVM<TKey, SelectOptionVM<TKey, object>>>(ServiceName, OperationType.Fetch, _logger);

        await result.FillAsync(async () =>
        {
            if (searchModel is null)
                searchModel = new();
            if (searchModel.Fields is null)
                searchModel.Fields = new();

            var data = new GridVM<TKey, SelectOptionVM<TKey, object>>(searchModel);

            var entities = await _repository.FetchAllAsync(ct);
            entities = Search(entities, searchModel.Fields);

            data.SearchModel.TotalCount = entities.Count();

            var list = EnumerableHelper.Search(entities, searchModel)
                .Select(e => _mapper.Map<SelectOptionVM<TKey, object>>(e))
                .ToList();

            data.List = list;

            result.Set(StatusCode.Succeeded, data);
        });

        return result;
    }
}
