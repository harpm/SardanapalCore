
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.Extensions.Logging;
using Sardanapal.Contract.IModel;
using Sardanapal.Contract.IRepository;
using Sardanapal.Contract.IService;
using Sardanapal.Ef.Helper;
using Sardanapal.ViewModel.Models;
using Sardanapal.ViewModel.Response;

namespace Sardanapal.Service;

public abstract class EFPanelServiceBase<TRepository, TKey, TEntity, TSearchVM, TVM, TNewVM, TEditableVM>
    : EFCurdServiceBase<TRepository, TKey, TEntity, TSearchVM, TVM, TNewVM, TEditableVM>
    , IEFPanelService<TKey, TSearchVM, TVM, TNewVM, TEditableVM>
    where TRepository : IEFCrudRepository<TKey, TEntity>
    where TKey : IComparable<TKey>, IEquatable<TKey>
    where TEntity : class, IBaseEntityModel<TKey>, new()
    where TSearchVM : class, new()
    where TVM : class, new()
    where TNewVM : class, new()
    where TEditableVM : class, new()
{
    protected EFPanelServiceBase(TRepository repository, IMapper mapper, ILogger logger)
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

            var list = QueryHelper.Search(entities, searchModel)
                .ProjectTo<SelectOptionVM<TKey, object>>(_mapper.ConfigurationProvider)
                .ToList();

            data.List = list;

            result.Set(StatusCode.Succeeded, data);
        });

        return result;
    }
}
