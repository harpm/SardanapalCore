
using AutoMapper;
using Sardanapal.Contract.IModel;
using Sardanapal.Contract.IRepository;
using Sardanapal.Contract.IService;
using Sardanapal.ViewModel.Response;

namespace Sardanapal.Service;

public abstract class CachablePanelServiceBase<TRepository, TCacheRepository, TKey, TEntity, TSearchVM, TVM, TNewVM, TEditableVM>
    : PanelServiceBase<TRepository, TKey, TEntity, TSearchVM, TVM, TNewVM, TEditableVM>
    , IPanelService<TKey, TSearchVM, TVM, TNewVM, TEditableVM>
    where TRepository : ICrudRepository<TKey, TEntity>
    where TCacheRepository : ICrudRepository<TKey, TEntity>
    where TKey : IComparable<TKey>, IEquatable<TKey>
    where TEntity : class, IBaseEntityModel<TKey>, new()
    where TSearchVM : class, new()
    where TVM : class, new()
    where TNewVM : class, new()
    where TEditableVM : class, new()
{
    protected readonly TCacheRepository _cacheRepository;

    protected CachablePanelServiceBase(TRepository repository
        , IMapper mapper
        , TCacheRepository cacheRepository)
        : base(repository, mapper)
    {
        _cacheRepository = cacheRepository;
    }

    public override async Task<IResponse<TKey>> Add(TNewVM Model, CancellationToken ct = default)
    {
        IResponse<TKey> result = new Response<TKey>(ServiceName, OperationType.Add);

        await result.FillAsync(async () =>
        {
            var entityModel = _mapper.Map<TNewVM, TEntity>(Model);
            TKey addedId = await _repository.AddAsync(entityModel, ct);

            await _cacheRepository.AddAsync(entityModel, ct);

            result.Set(StatusCode.Succeeded, addedId);
        });

        return result;
    }

    public override async Task<IResponse<TVM>> Get(TKey Id, CancellationToken ct = default)
    {
        IResponse<TVM> result = new Response<TVM>(ServiceName, OperationType.Fetch);

        await result.FillAsync(async () =>
        {
            var fetchModel = await _cacheRepository.FetchByIdAsync(Id, ct);

            if (fetchModel == null)
            {
                fetchModel = await _repository.FetchByIdAsync(Id, ct);
            }

            TVM model = _mapper.Map<TEntity, TVM>(fetchModel);
            result.Set(StatusCode.Succeeded, model);
        });

        return result;
    }

    public override async Task<IResponse<TEditableVM>> GetEditable(TKey Id, CancellationToken ct = default)
    {
        IResponse<TEditableVM> result = new Response<TEditableVM>(ServiceName, OperationType.Fetch);

        await result.FillAsync(async () =>
        {
            var fetchModel = await _cacheRepository.FetchByIdAsync(Id, ct);

            if (fetchModel == null)
            {
                fetchModel = await _repository.FetchByIdAsync(Id, ct);
            }

            TEditableVM model = _mapper.Map<TEntity, TEditableVM>(fetchModel);
            result.Set(StatusCode.Succeeded, model);
        });

        return result;
    }

    public override async Task<IResponse<bool>> Edit(TKey Id, TEditableVM Model, CancellationToken ct = default)
    {
        IResponse<bool> result = new Response<bool>(ServiceName, OperationType.Edit);

        await result.FillAsync(async () =>
        {
            var editedModel = _mapper.Map<TEditableVM, TEntity>(Model);
            var data = await _repository.UpdateAsync(Id, editedModel, ct);

            await _cacheRepository.UpdateAsync(Id, editedModel, ct);

            result.Set(data ? StatusCode.Succeeded : StatusCode.Failed, data);
        });

        return result;
    }

    public override async Task<IResponse<bool>> Delete(TKey Id, CancellationToken ct = default)
    {
        IResponse<bool> result = new Response<bool>(ServiceName, OperationType.Edit);

        await result.FillAsync(async () =>
        {
            var data = await _repository.DeleteAsync(Id, ct);

            await _cacheRepository.DeleteAsync(Id, ct);

            result.Set(data ? StatusCode.Succeeded : StatusCode.Failed, data);
        });

        return result;
    }
}
