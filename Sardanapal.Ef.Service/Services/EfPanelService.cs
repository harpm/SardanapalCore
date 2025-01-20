using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Sardanapal.Ef.Helper;
using Sardanapal.ViewModel.Models;
using Sardanapal.ViewModel.Response;
using Sardanapal.Ef.Service.Services;
using Sardanapal.Contract.IModel;
using Sardanapal.Contract.IService;

namespace Sardanapal.Ef.Services.Services;

public abstract class EfPanelService<TContext, TKey, TEntity, TListItemVM, TSearchVM, TVM, TNewVM, TEditableVM>
    : EfCrudService<TContext, TKey, TEntity, TListItemVM, TSearchVM, TVM, TNewVM, TEditableVM>
    , IPanelService<TKey, TSearchVM, TVM, TNewVM, TEditableVM>
    where TContext : DbContext
    where TKey : IEquatable<TKey>, IComparable<TKey>
    where TEntity : class, IBaseEntityModel<TKey>
    where TListItemVM : BaseListItem<TKey>
    where TVM : class, new()
    where TSearchVM : class, new()
    where TNewVM : class, new()
    where TEditableVM : class, new()
{

    public EfPanelService(TContext unitOfWork, IMapper mapper, IRequestService context)
        : base(unitOfWork, mapper, context)
    {

    }

    protected virtual IQueryable<TEntity> GetCurrentService()
    {
        return CurrentService;
    }

    protected virtual IQueryable<TEntity> Search(IQueryable<TEntity> query, TSearchVM SearchModel)
    {
        return query;
    }

    // TODO: Needs Review
    public virtual async Task<IResponse<GridVM<TKey, SelectOptionVM<TKey, object>>>> GetDictionary(GridSearchModelVM<TKey, TSearchVM> SearchModel = null)
    {
        var Result = new Response<GridVM<TKey, SelectOptionVM<TKey, object>>>(ServiceName, OperationType.Fetch);

        return await Result.FillAsync(async () =>
        {
            if (SearchModel == null)
                SearchModel = new GridSearchModelVM<TKey, TSearchVM>();

            var ResultValue = new GridVM<TKey, SelectOptionVM<TKey, object>>(SearchModel);

            var QList = GetCurrentService().AsNoTracking();

            if (SearchModel != null && SearchModel.Fields != null)
            {
                QList = Search(QList, SearchModel.Fields);
            }

            ResultValue.SearchModel.TotalCount = await QList.CountAsync();

            QList = QueryHelper.Search(QList, SearchModel);

            ResultValue.List = await QList
                .ProjectTo<SelectOptionVM<TKey, object>>(Mapper.ConfigurationProvider)
                .ToListAsync();

            Result.Set(StatusCode.Succeeded, ResultValue);
        });
    }
}
