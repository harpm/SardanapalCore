
using AutoMapper;
using Microsoft.Extensions.Logging;
using Sardanapal.Contract.IModel;
using Sardanapal.Contract.IRepository;
using Sardanapal.Contract.IService;
using Sardanapal.Share.Helpers;
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
        GetDictionary(GridSearchModelVM<TKey, TSearchVM> searchModel = null, CancellationToken ct = default);

    public virtual async Task<IResponse<byte[]>> GetExcel<T>(GridSearchModelVM<TKey, TSearchVM> searchModel = null, CancellationToken ct = default)
        where T : class
    {
        IResponse<byte[]> result = new Response<byte[]>(ServiceName, OperationType.Fetch, _logger);

        await result.FillAsync(async () =>
        {
            GridSearchModelVM<TKey, TSearchVM> exportModel = searchModel is null
                ? new GridSearchModelVM<TKey, TSearchVM>()
                : searchModel with { PageSize = 0, PageIndex = 0 };

            IResponse<GridVM<TKey, T>> grid = await GetAll<T>(exportModel, ct);

            if (!grid.IsSuccess)
            {
                result.Set(grid.StatusCode, grid.DeveloperMessages ?? [], grid.UserMessage);
                return;
            }

            byte[] excel = ExcelHelper.ToExcel(grid.Data?.List, exportModel.Columns);

            result.Set(StatusCode.Succeeded, excel);
        });

        return result;
    }
}
