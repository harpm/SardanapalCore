using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Sardanapal.Ef.Services.Services;
using Sardanapal.Contract.IService;
using Sardanapal.Contract.IModel;
using Sardanapal.ViewModel.Models;
using Sardanapal.ViewModel.Response;

namespace Sardanapal.Ef.Service.Services;

public abstract class EfCachablePanelService<TContext, TCachService, TKey, TEntity, TListItemVM, TSearchVM, TVM, TNewVM, TEditableVM>
    : EfPanelService<TContext, TKey, TEntity, TListItemVM, TSearchVM, TVM, TNewVM, TEditableVM>
    , IPanelService<TKey, TSearchVM, TVM, TNewVM, TEditableVM>
    where TContext : DbContext
    where TCachService : class, ICacheService<TEntity, TKey, TSearchVM, TVM, TNewVM, TEditableVM>
    where TKey : IEquatable<TKey>, IComparable<TKey>
    where TEntity : class, IBaseEntityModel<TKey>, new()
    where TListItemVM : BaseListItem<TKey>
    where TVM : class, ICachModel<TKey>, new()
    where TSearchVM : class, new()
    where TNewVM : class, ICachModel<TKey>, new()
    where TEditableVM : class, ICachModel<TKey>, new()
{
    protected TCachService _cachService;

    protected EfCachablePanelService(TContext unitOfWork
        , IMapper mapper
        , IRequestService context
        , TCachService cachService)
        : base(unitOfWork, mapper, context)
    {
        _cachService = cachService;
    }

    public override async Task<IResponse<TVM>> Get(TKey Id)
    {
        var result = await _cachService.Get(Id);
        if (result.StatusCode != StatusCode.Succeeded)
        {
            result = new Response<TVM>(ServiceName);

            await result.FillAsync(async () =>
            {
                var model = await GetCurrentService().AsNoTracking()
                    .Where(x => x.Id.Equals(Id))
                    .FirstOrDefaultAsync();
                if (model != null)
                {
                    await _cachService.Add(model);

                    var resultData = Mapper.Map<TEntity, TVM>(model);

                    result.Set(StatusCode.Succeeded, resultData);
                }
                else
                {
                    result.Set(StatusCode.NotExists);
                }
            });
        }

        return result;
    }

    public override Task<IResponse<GridVM<TKey, SelectOptionVM<TKey, object>, TSearchVM>>> GetDictionary(GridSearchModelVM<TKey, TSearchVM> SearchModel = null)
    {
        return _cachService.GetDictionary(SearchModel);
    }

    public override async Task<IResponse<bool>> Edit(TKey Id, TEditableVM Model)
    {
        var result = await base.Edit(Id, Model);

        if (result.IsSuccess)
        {
            var cResult = await _cachService.Delete(Id);
        }

        return result;
    }

    public async override Task<IResponse<bool>> Delete(TKey Id)
    {
        var result = await base.Delete(Id);

        if (result.IsSuccess)
        {
            await result.FillAsync(async () =>
            {
                var secondRes = await _cachService.Delete(Id);
                if (!secondRes.IsSuccess)
                {
                    result = secondRes;
                }
            });
        }

        return result;
    }
}
