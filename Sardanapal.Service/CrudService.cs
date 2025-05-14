
using AutoMapper;
using Sardanapal.Contract.IModel;
using Sardanapal.Contract.IRepository;
using Sardanapal.Contract.IService;
using Sardanapal.ViewModel.Models;
using Sardanapal.ViewModel.Response;

namespace Sardanapal.Service;

public abstract class CrudServiceBase<TRepository, TKey, TEntity, TSearchVM, TVM, TNewVM, TEditableVM>
    : ICrudService<TKey, TSearchVM, TVM, TNewVM, TEditableVM>
    where TRepository : ICrudRepository<TKey, TEntity>
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

    protected CrudServiceBase(TRepository repository, IMapper mapper)
    {
        this._repository = repository;
        this._mapper = mapper;
    }
    
    public virtual async Task<IResponse<TKey>> Add(TNewVM Model)
    {
        IResponse<TKey> result = new Response<TKey>(ServiceName, OperationType.Add);

        await result.FillAsync(async () =>
        {
            var entityModel = _mapper.Map<TNewVM, TEntity>(Model);
            TKey addedId = await _repository.AddAsync(entityModel);
            result.Set(StatusCode.Succeeded, addedId);
        });

        return result;
    }

    public virtual async Task<IResponse<TVM>> Get(TKey Id)
    {
        IResponse<TVM> result = new Response<TVM>(ServiceName, OperationType.Fetch);

        await result.FillAsync(async () =>
        {
            var fetchModel = await _repository.FetchByIdAsync(Id);
            TVM model = _mapper.Map<TEntity, TVM>(fetchModel);
            result.Set(StatusCode.Succeeded, model);
        });

        return result;
    }

    public abstract Task<IResponse<GridVM<TKey, T>>> GetAll<T>(GridSearchModelVM<TKey, TSearchVM> SearchModel = null) where T : class;

    public virtual async Task<IResponse<TEditableVM>> GetEditable(TKey Id)
    {
        IResponse<TEditableVM> result = new Response<TEditableVM>(ServiceName, OperationType.Fetch);

        await result.FillAsync(async () =>
        {
            var fetchModel = await _repository.FetchByIdAsync(Id);
            TEditableVM model = _mapper.Map<TEntity, TEditableVM>(fetchModel);
            result.Set(StatusCode.Succeeded, model);
        });

        return result;
    }

    public virtual async Task<IResponse<bool>> Edit(TKey Id, TEditableVM Model)
    {
        IResponse<bool> result = new Response<bool>(ServiceName, OperationType.Edit);

        await result.FillAsync(async () =>
        {
            var editedModel = _mapper.Map<TEditableVM, TEntity>(Model);
            var data = await _repository.UpdateAsync(Id, editedModel);
            
            result.Set(data ? StatusCode.Succeeded : StatusCode.Failed, data);
        });

        return result;
    }

    public virtual async Task<IResponse<bool>> Delete(TKey Id)
    {
        IResponse<bool> result = new Response<bool>(ServiceName, OperationType.Edit);

        await result.FillAsync(async () =>
        {
            var data = await _repository.DeleteAsync(Id);

            result.Set(data ? StatusCode.Succeeded : StatusCode.Failed, data);
        });

        return result;
    }
}
