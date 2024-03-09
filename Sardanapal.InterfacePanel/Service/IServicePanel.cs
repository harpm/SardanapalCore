using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Sardanapal.DomainModel.Domain;
using Sardanapal.Ef.Helper;
using Sardanapal.ViewModel.Models;
using Sardanapal.ViewModel.Response;

namespace Sardanapal.InterfacePanel.Service;

public interface _IServicePanel<TKey, TListItemVM, TSearchVM, TVM, TNewVM, TEditableVM>
    where TKey : IEquatable<TKey>, IComparable<TKey>
    where TListItemVM : BaseListItem<TKey>
    where TSearchVM : class
    where TNewVM : class
    where TEditableVM : class
{
    Task<Response<TVM>> Get(TKey Id);
    Task<Response<GridVM<T, TSearchVM>>> GetAll<T>(GridSearchModelVM<TSearchVM> SearchModel = null) where T : class;
    Task<Response<TKey>> Add(TNewVM Model);
    Task<Response<TEditableVM>> GetEditable(TKey Id);
    Task<Response> Edit(TKey Id, TEditableVM Model);
    Task<Response> Delete(TKey Id);
    Task<Response<GridVM<SelectOptionVM<TKey, object>, TSearchVM>>> GetDictionary(GridSearchModelVM<TSearchVM> SearchModel = null);
}

public abstract class _ServicePanel<TContext, TKey, TEntity, TListItemVM, TSearchVM, TVM, TNewVM, TEditableVM> : _IServicePanel<TKey, TListItemVM, TSearchVM, TVM, TNewVM, TEditableVM>
    where TContext : DbContext
    where TKey : IEquatable<TKey>, IComparable<TKey>
    where TEntity : class, IBaseEntityModel<TKey>
    where TListItemVM : BaseListItem<TKey>
    where TVM : class
    where TSearchVM : class
    where TNewVM : class
    where TEditableVM : class
{

    public abstract string ServiceName { get; }

    protected TContext UnitOfWork;
    protected IMapper Mapper;
    protected IRequestService CurrentRequest;
    protected IQueryable<TEntity> CurrentService;

    public _ServicePanel(TContext unitOfWork, IMapper _Mapper, IRequestService _Context)
    {
        CurrentRequest = _Context;
        Mapper = _Mapper;
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

    public virtual async Task<Response<TVM>> Get(TKey Id)
    {
        var Result = new Response<TVM>(ServiceName, OperationType.Fetch);

        try
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
        }
        catch (Exception ex)
        {
            Result.Set(StatusCode.Exception, ex);
        }

        return Result;
    }

    public virtual async Task<Response<GridVM<T, TSearchVM>>> GetAll<T>(GridSearchModelVM<TSearchVM> SearchModel = null) where T : class
    {
        var Result = new Response<GridVM<T, TSearchVM>>(ServiceName, OperationType.Fetch);
        var ResultValue = new GridVM<T, TSearchVM>(SearchModel);

        try
        {
            var QList = GetCurrentService().AsNoTracking();

            if (SearchModel != null && SearchModel.Fields != null)
            {
                QList = Search(QList, SearchModel.Fields);
            }

            ResultValue.TotalCount = await QList.CountAsync();

            QList = QueryHelper.Search(QList, SearchModel);

            ResultValue.List = await QList.ProjectTo<T>(Mapper.ConfigurationProvider).ToListAsync();

            Result.Set(StatusCode.Succeeded, ResultValue);
        }
        catch (Exception ex)
        {
            Result.Set(StatusCode.Exception, ex);
        }

        return Result;
    }

    public virtual async Task<Response<TKey>> Add(TNewVM Model)
    {
        var Result = new Response<TKey>(ServiceName, OperationType.Add);

        try
        {
            var Item = Mapper.Map<TEntity>(Model);
            await UnitOfWork.AddAsync(Item);
            await UnitOfWork.SaveChangesAsync();

            Result.Set(StatusCode.Succeeded, Item.Id);
        }
        catch (Exception ex)
        {
            Result.Set(StatusCode.Exception, ex);
        }

        return Result;
    }

    public virtual async Task<Response<TEditableVM>> GetEditable(TKey Id)
    {
        var Result = new Response<TEditableVM>(ServiceName, OperationType.Fetch);

        try
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
        }
        catch (Exception ex)
        {
            Result.Set(StatusCode.Exception, ex);
        }

        return Result;
    }

    public virtual async Task<Response> Edit(TKey Id, TEditableVM Model)
    {
        var Result = new Response(ServiceName, OperationType.Edit);

        try
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

        }
        catch (Exception ex)
        {
            Result.Set(StatusCode.Exception, ex);
        }

        return Result;
    }

    public virtual async Task<Response> Delete(TKey Id)
    {
        var Result = new Response(ServiceName, OperationType.Delete);

        try
        {
            var Item = await GetCurrentService().Where(x => x.Id.Equals(Id)).FirstOrDefaultAsync();
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
        }
        catch (Exception ex)
        {
            Result.Set(StatusCode.Exception, ex);
        }

        return Result;
    }

    // Needs Review
    public async Task<Response<GridVM<SelectOptionVM<TKey, object>, TSearchVM>>> GetDictionary(GridSearchModelVM<TSearchVM> SearchModel = null)
    {
        var Result = new Response<GridVM<SelectOptionVM<TKey, object>, TSearchVM>>(ServiceName, OperationType.Fetch);
        var ResultValue = new GridVM<SelectOptionVM<TKey, object>, TSearchVM>(SearchModel);

        try
        {
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
        }
        catch (Exception ex)
        {
            Result.Set(StatusCode.Exception, ex);
        }

        return Result;
    }
}