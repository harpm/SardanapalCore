using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Sardanapal.DomainModel.Domain;
using Sardanapal.Ef.Helper;
using Sardanapal.InterfacePanel.Service;
using Sardanapal.ViewModel.Models;
using Sardanapal.ViewModel.Response;
using Sardanapal.Interface.IService;
using Sardanapal.Ef.Service.Services;

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

    public virtual async Task<IResponse<TEditableVM>> GetEditable(TKey Id)
    {
        var Result = new Response<TEditableVM>(ServiceName, OperationType.Fetch);

        return await Result.Create(async () =>
        {
            var Item = await GetCurrentService().AsNoTracking()
                .Where(x => x.Id.Equals(Id))
                .ProjectTo<TEditableVM>(Mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            if (Item != null)
            {
                Result.Set(StatusCode.Succeeded, Item);
            }
            else
            {
                Result.Set(StatusCode.NotExists);
            }

            return Result;
        });
    }

    // TODO: Needs Review
    public async Task<IResponse<GridVM<SelectOptionVM<TKey, object>, TSearchVM>>> GetDictionary(GridSearchModelVM<TSearchVM> SearchModel = null)
    {
        var Result = new Response<GridVM<SelectOptionVM<TKey, object>, TSearchVM>>(ServiceName, OperationType.Fetch);

        return await Result.Create(async () =>
        {
            var ResultValue = new GridVM<SelectOptionVM<TKey, object>, TSearchVM>(SearchModel);

            var QList = GetCurrentService().AsNoTracking();

            if (SearchModel != null && SearchModel.Fields != null)
            {
                QList = Search(QList, SearchModel.Fields);
            }

            ResultValue.TotalCount = await QList.CountAsync();

            QList = QueryHelper.Search(QList, SearchModel);

            ResultValue.List = await QList
                .ProjectTo<SelectOptionVM<TKey, object>>(Mapper.ConfigurationProvider)
                .ToListAsync();

            Result.Set(StatusCode.Succeeded, ResultValue);

            return Result;
        });
    }
}
