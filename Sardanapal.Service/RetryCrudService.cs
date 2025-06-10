
using AutoMapper;
using Sardanapal.Contract.IModel;
using Sardanapal.Contract.IRepository;
using Sardanapal.Share.Utilities;
using Sardanapal.ViewModel.Response;

namespace Sardanapal.Service;

public abstract class RetryCrudServiceBase<TRepository, TKey, TEntity, TListItemVM, TSearchVM, TVM, TNewVM, TEditableVM>
    : CrudServiceBase<TRepository, TKey, TEntity, TSearchVM, TVM, TNewVM, TEditableVM>
    where TRepository : ICrudRepository<TKey, TEntity>
    where TKey : IComparable<TKey>, IEquatable<TKey>
    where TEntity : class, IBaseEntityModel<TKey>, new()
    where TSearchVM : class, new()
    where TVM : class, new()
    where TNewVM : class, new()
    where TEditableVM : class, new()
{
    protected abstract int _secondsBetweenRetries { get; }
    protected abstract int _retryCount { get; }

    protected RetryCrudServiceBase(TRepository repository, IMapper mapper)
        : base(repository, mapper)
    {
        
    }

    public override async Task<IResponse<TKey>> Add(TNewVM Model, CancellationToken ct = default)
    {
        IResponse<TKey> result = new Response<TKey>(ServiceName, OperationType.Add);

        await RetryHelper.RetryUntillAsync(_secondsBetweenRetries, _retryCount, async () =>
        {
            await result.FillAsync(async () =>
            {
                var entityModel = _mapper.Map<TNewVM, TEntity>(Model);
                TKey addedId = await _repository.AddAsync(entityModel);
                result.Set(StatusCode.Succeeded, addedId);
            });

            return result.IsSuccess;
        }, ct);

        return result;
    }

    public override async Task<IResponse<TVM>> Get(TKey Id, CancellationToken ct = default)
    {
        IResponse<TVM> result = new Response<TVM>(ServiceName, OperationType.Fetch);

        await RetryHelper.RetryUntillAsync(_secondsBetweenRetries, _retryCount, async () =>
        {
            await result.FillAsync(async () =>
            {
                var fetchModel = await _repository.FetchByIdAsync(Id);
                TVM model = _mapper.Map<TEntity, TVM>(fetchModel);
                result.Set(StatusCode.Succeeded, model);
            });

            return result.IsSuccess;
        }, ct);

        return result;
    }

    public override async Task<IResponse<TEditableVM>> GetEditable(TKey Id, CancellationToken ct = default)
    {
        IResponse<TEditableVM> result = new Response<TEditableVM>(ServiceName, OperationType.Fetch);

        await RetryHelper.RetryUntillAsync(_secondsBetweenRetries, _retryCount, async () =>
        {
            await result.FillAsync(async () =>
            {
                var fetchModel = await _repository.FetchByIdAsync(Id);
                TEditableVM model = _mapper.Map<TEntity, TEditableVM>(fetchModel);
                result.Set(StatusCode.Succeeded, model);
            });

            return result.IsSuccess;
        }, ct);

        return result;
    }

    public override async Task<IResponse<bool>> Edit(TKey Id, TEditableVM Model, CancellationToken ct = default)
    {
        IResponse<bool> result = new Response<bool>(ServiceName, OperationType.Edit);

        await RetryHelper.RetryUntillAsync(_secondsBetweenRetries, _retryCount, async () =>
        {
            await result.FillAsync(async () =>
            {
                var editedModel = _mapper.Map<TEditableVM, TEntity>(Model);
                var data = await _repository.UpdateAsync(Id, editedModel);

                result.Set(data ? StatusCode.Succeeded : StatusCode.Failed, data);
            });
            return result.IsSuccess;
        }, ct);

        return result;
    }

    public override async Task<IResponse<bool>> Delete(TKey Id, CancellationToken ct = default)
    {
        IResponse<bool> result = new Response<bool>(ServiceName, OperationType.Edit);

        await RetryHelper.RetryUntillAsync(_secondsBetweenRetries, _retryCount, async () =>
        {
            await result.FillAsync(async () =>
            {
                var data = await _repository.DeleteAsync(Id);

                result.Set(data ? StatusCode.Succeeded : StatusCode.Failed, data);
            });

            return result.IsSuccess;
        }, ct);

        return result;
    }
}
