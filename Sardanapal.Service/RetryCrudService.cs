
using AutoMapper;
using Microsoft.Extensions.Logging;
using Sardanapal.Contract.IModel;
using Sardanapal.Contract.IRepository;
using Sardanapal.Localization;
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

    protected RetryCrudServiceBase(TRepository repository, IMapper mapper, ILogger logger)
        : base(repository, mapper, logger)
    {

    }

    public override async Task<IResponse<TKey>> Add(TNewVM model, CancellationToken ct = default)
    {
        IResponse<TKey> result = new Response<TKey>(ServiceName, OperationType.Add, _logger);

        await result.FillAsync(async () =>
        {
            await RetryHelper.RetryUntillAsync(_secondsBetweenRetries, _retryCount, async () =>
            {
                var entityModel = _mapper.Map<TNewVM, TEntity>(model);
                TKey addedId = await _repository.AddAsync(entityModel);
                result.Set(StatusCode.Succeeded, addedId);

                return result.IsSuccess;
            }, ct);
        });

        return result;
    }

    public override async Task<IResponse<bool>> Edit(TKey id, TEditableVM model, CancellationToken ct = default)
    {
        IResponse<bool> result = new Response<bool>(ServiceName, OperationType.Edit, _logger);

        await result.FillAsync(async () =>
        {
            await RetryHelper.RetryUntillAsync(_secondsBetweenRetries, _retryCount, async () =>
            {
                var entity = await _repository.FetchByIdAsync(id, ct);
                if (entity != null)
                {
                    entity = _mapper.Map(model, entity);
                    var data = await _repository.UpdateAsync(id, entity);
                    result.Set(data ? StatusCode.Succeeded : StatusCode.Failed, data);
                }
                else
                {
                    result.Set(StatusCode.NotExists, [], Messages.NotExist);
                }

                return result.StatusCode != StatusCode.Exception;
            }, ct);
        });

        return result;
    }

    public override async Task<IResponse<bool>> Delete(TKey id, CancellationToken ct = default)
    {
        IResponse<bool> result = new Response<bool>(ServiceName, OperationType.Edit, _logger);

        await result.FillAsync(async () =>
        {
            await RetryHelper.RetryUntillAsync(_secondsBetweenRetries, _retryCount, async () =>
            {
                var data = await _repository.DeleteAsync(id);

                result.Set(data ? StatusCode.Succeeded : StatusCode.Failed, data);

                return result.IsSuccess;
            }, ct);
        });

        return result;
    }
}
