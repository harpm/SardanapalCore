using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Sardanapal.Contract.IModel;
using Sardanapal.Contract.IService;
using Sardanapal.Ef.Helper;
using Sardanapal.Share.Utilities;
using Sardanapal.ViewModel.Models;
using Sardanapal.ViewModel.Response;

namespace Sardanapal.Ef.Service.Services;

public abstract class EfRetryCrudService<TContext, TKey, TEntity, TListItemVM, TSearchVM, TVM, TNewVM, TEditableVM>
    : ICrudService<TKey, TSearchVM, TVM, TNewVM, TEditableVM>
    where TContext : DbContext
    where TKey : IEquatable<TKey>, IComparable<TKey>
    where TEntity : class, IBaseEntityModel<TKey>
    where TListItemVM : BaseListItem<TKey>
    where TVM : class, new()
    where TSearchVM : class, new()
    where TNewVM : class, new()
    where TEditableVM : class, new()
{
    public abstract string ServiceName { get; }

    protected TContext UnitOfWork;
    protected IMapper Mapper;
    protected IRequestService CurrentRequest;
    protected IQueryable<TEntity> CurrentService;

    public EfRetryCrudService(TContext unitOfWork, IMapper mapper, IRequestService context)
    {
        CurrentRequest = context;
        Mapper = mapper;
        UnitOfWork = unitOfWork;
        CurrentService = UnitOfWork.Set<TEntity>();
    }

    protected virtual IQueryable<TEntity> GetCurrentService()
    {
        return CurrentService;
    }

    protected virtual IQueryable<TEntity> Search(IQueryable<TEntity> query, TSearchVM SearchModel)
    {
        return query;
    }

    public virtual async Task<IResponse<TVM>> Get(TKey Id)
    {
        IResponse<TVM> Result = new Response<TVM>(ServiceName, OperationType.Fetch);
        await RetryService.RetryUntillSuccessAsync(1, async () =>
        {
            Result = await Result.FillAsync(async () =>
            {
                var Item = await GetCurrentService().AsNoTracking()
                    .Where(x => x.Id.Equals(Id))
                    .ProjectTo<TVM>(Mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync();

                if (Item != null)
                {
                    Result.Set(StatusCode.Succeeded, Item);
                }
                else
                {
                    Result.Set(StatusCode.NotExists);
                }
            });

            return Result.IsSuccess;
        });

        return Result;
    }

    public virtual async Task<IResponse<GridVM<TKey, T, TSearchVM>>> GetAll<T>(GridSearchModelVM<TKey, TSearchVM> SearchModel = null) where T : class
    {
        IResponse<GridVM<TKey, T, TSearchVM>> Result = new Response<GridVM<TKey, T, TSearchVM>>(ServiceName, OperationType.Fetch);

        await RetryService.RetryUntillSuccessAsync(1, async () =>
        {
            Result = await Result.FillAsync(async () =>
            {
                if (SearchModel == null)
                    SearchModel = new GridSearchModelVM<TKey, TSearchVM>();

                var ResultValue = new GridVM<TKey, T, TSearchVM>(SearchModel);

                var QList = GetCurrentService().AsNoTracking();

                if (SearchModel != null && SearchModel.Fields != null)
                {
                    QList = Search(QList, SearchModel.Fields);
                }

                ResultValue.SearchModel.TotalCount = await QList.CountAsync();

                QList = QueryHelper.Search(QList, SearchModel);

                ResultValue.List = await QList.ProjectTo<T>(Mapper.ConfigurationProvider).ToListAsync();

                Result.Set(StatusCode.Succeeded, ResultValue);
            });

            return Result.IsSuccess;
        });

        return Result;
    }

    public virtual async Task<IResponse<TKey>> Add(TNewVM Model)
    {
        IResponse<TKey> Result = new Response<TKey>(ServiceName, OperationType.Add);

        await RetryService.RetryUntillSuccessAsync(1, async () =>
        {
            Result = await Result.FillAsync(async () =>
            {
                var Item = Mapper.Map<TEntity>(Model);
                await UnitOfWork.AddAsync(Item);
                await UnitOfWork.SaveChangesAsync();

                Result.Set(StatusCode.Succeeded, Item.Id);
            });
            return Result.IsSuccess;
        });

        return Result;
    }

    public virtual async Task<IResponse<TEditableVM>> GetEditable(TKey Id)
    {
        IResponse<TEditableVM> Result = new Response<TEditableVM>(ServiceName, OperationType.Fetch);

        await RetryService.RetryUntillSuccessAsync(1, async () =>
        {
            Result = await Result.FillAsync(async () =>
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
            });

            return Result.IsSuccess;
        });

        return Result;
    }

    public virtual async Task<IResponse<bool>> Edit(TKey Id, TEditableVM Model)
    {
        IResponse<bool> Result = new Response(ServiceName, OperationType.Edit);

        await RetryService.RetryUntillSuccessAsync(1, async () =>
        {
            Result = await Result.FillAsync(async () =>
            {
                var Item = await GetCurrentService()
                        .Where(x => x.Id.Equals(Id))
                        .FirstOrDefaultAsync();

                if (Item != null)
                {
                    Mapper.Map(Model, Item);
                    await UnitOfWork.SaveChangesAsync();

                    Result.Set(StatusCode.Succeeded, true);
                }
                else
                {
                    Result.Set(StatusCode.NotExists);
                }
            });

            return Result.IsSuccess;
        });

        return Result;
    }

    public virtual async Task<IResponse<bool>> Delete(TKey Id)
    {
        IResponse<bool> Result = new Response(ServiceName, OperationType.Delete);

        await RetryService.RetryUntillSuccessAsync(1, async () =>
        {
            Result = await Result.FillAsync(async () =>
            {
                var Item = await GetCurrentService()
                    .Where(x => x.Id.Equals(Id))
                    .FirstOrDefaultAsync();
                if (Item != null)
                {
                    UnitOfWork.Remove(Item);
                    await UnitOfWork.SaveChangesAsync();

                    Result.Set(StatusCode.Succeeded);
                }
                else
                {
                    Result.Set(StatusCode.NotExists);
                }
            });

            return Result.IsSuccess;
        });

        return Result;
    }
}
