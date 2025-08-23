
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Sardanapal.Contract.IModel;
using Sardanapal.Contract.IRepository;
using Sardanapal.Contract.IService;
using Sardanapal.Ef.Helper;
using Sardanapal.Localization;
using Sardanapal.ViewModel.Models;
using Sardanapal.ViewModel.Response;

namespace Sardanapal.Service;

public abstract class EFCurdServiceBase<TRepository, TKey, TEntity, TSearchVM, TVM, TNewVM, TEditableVM>
    : IEFCrudService<TKey, TSearchVM, TVM, TNewVM, TEditableVM>
    where TRepository : IEFCrudRepository<TKey, TEntity>
    where TKey : IComparable<TKey>, IEquatable<TKey>
    where TEntity : class, IBaseEntityModel<TKey>, new()
    where TSearchVM : class, new()
    where TVM : class, new()
    where TNewVM : class, new()
    where TEditableVM : class, new()
{
    protected abstract string ServiceName { get; }
    protected readonly TRepository _repository;
    protected readonly IMapper _mapper;

    protected EFCurdServiceBase(TRepository repository, IMapper mapper)
    {
        this._repository = repository;
        this._mapper = mapper;
    }

    protected abstract IQueryable<TEntity> Search(IQueryable<TEntity> entities, TSearchVM searchVM);


    protected virtual Task FillEntityKey(TEntity entity)
    {
        return Task.CompletedTask;
    }

    public virtual async Task<IResponse<TKey>> Add(TNewVM Model, CancellationToken ct = default)
    {
        IResponse<TKey> result = new Response<TKey>(ServiceName, OperationType.Add);

        await result.FillAsync(async () =>
        {
            var entityModel = _mapper.Map<TNewVM, TEntity>(Model);
            await FillEntityKey(entityModel);
            TKey addedId = await _repository.AddAsync(entityModel, ct);
            await _repository.SaveChangesAsync(ct);
            result.Set(StatusCode.Succeeded, addedId);
        });

        return result;
    }

    public virtual async Task<IResponse<TVM>> Get(TKey Id, CancellationToken ct = default)
    {
        IResponse<TVM> result = new Response<TVM>(ServiceName, OperationType.Fetch);

        await result.FillAsync(async () =>
        {
            var fetchModel = await _repository.FetchByIdAsync(Id, ct);
            if (fetchModel != null)
            {
                TVM model = _mapper.Map<TEntity, TVM>(fetchModel);
                result.Set(StatusCode.Succeeded, model);
            }
            else
            {
                result.Set(StatusCode.NotExists, [], Messages.NotExist);
            }
        });

        return result;
    }

    public virtual async Task<IResponse<GridVM<TKey, T>>> GetAll<T>(GridSearchModelVM<TKey, TSearchVM> SearchModel = null, CancellationToken ct = default) where T : class
    {
        IResponse<GridVM<TKey, T>> result = new Response<GridVM<TKey, T>>(ServiceName, OperationType.Fetch);

        await result.FillAsync(async () =>
        {
            if (SearchModel is null)
                SearchModel = new();
            if (SearchModel.Fields is null)
                SearchModel.Fields = new();

            var data = new GridVM<TKey, T>(SearchModel);

            var entities = await _repository.FetchAllAsync(ct);
            entities = Search(entities, SearchModel.Fields);

            data.SearchModel.TotalCount = entities.Count();

            var list = QueryHelper.Search(entities, SearchModel)
                .ProjectTo<T>(_mapper.ConfigurationProvider)
                .ToList();
            data.List = list;

            result.Set(StatusCode.Succeeded, data);
        });

        return result;
    }

    public virtual async Task<IResponse<TEditableVM>> GetEditable(TKey Id, CancellationToken ct = default)
    {
        IResponse<TEditableVM> result = new Response<TEditableVM>(ServiceName, OperationType.Fetch);

        await result.FillAsync(async () =>
        {
            var fetchModel = await _repository.FetchByIdAsync(Id, ct);
            TEditableVM model = _mapper.Map<TEntity, TEditableVM>(fetchModel);
            result.Set(StatusCode.Succeeded, model);
        });

        return result;
    }

    public virtual async Task<IResponse<bool>> Edit(TKey Id, TEditableVM Model, CancellationToken ct = default)
    {
        IResponse<bool> result = new Response<bool>(ServiceName, OperationType.Edit);

        await result.FillAsync(async () =>
        {
            var entity = await _repository.FetchByIdAsync(Id, ct);
            if (entity != null)
            {
                _mapper.Map(Model, entity);
                var data = await _repository.UpdateAsync(Id, entity, ct);
                result.Set(data ? StatusCode.Succeeded : StatusCode.Failed, data);
            }
            else
            {
                result.Set(StatusCode.NotExists, [], Messages.NotExist);
            }
        });

        return result;
    }

    public virtual async Task<IResponse<bool>> Delete(TKey Id, CancellationToken ct = default)
    {
        IResponse<bool> result = new Response<bool>(ServiceName, OperationType.Edit);

        await result.FillAsync(async () =>
        {
            var data = await _repository.DeleteAsync(Id, ct);

            result.Set(data ? StatusCode.Succeeded : StatusCode.Failed, data);
        });

        return result;
    }
}
