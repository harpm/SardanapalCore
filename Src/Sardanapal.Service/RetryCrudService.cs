
using AutoMapper;
using Microsoft.Extensions.Configuration;
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
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Maximum number of retry attempts for a failing operation.
    /// Override to customize, or provide via IConfiguration key "Sardanapal:Retry:MaxRetries". Defaults to 3.
    /// </summary>
    protected virtual int MaxRetries => ReadRetryConfig("Sardanapal:Retry:MaxRetries", 3);

    /// <summary>
    /// Seconds to wait between retry attempts.
    /// Override to customize, or provide via IConfiguration key "Sardanapal:Retry:SecondsBetweenRetries". Defaults to 2.
    /// </summary>
    protected virtual int SecondsBetweenRetries => ReadRetryConfig("Sardanapal:Retry:SecondsBetweenRetries", 2);

    protected RetryCrudServiceBase(TRepository repository, IMapper mapper, ILogger logger, IConfiguration configuration = null)
        : base(repository, mapper, logger)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Reads an integer retry setting from IConfiguration when available and valid,
    /// otherwise falls back to the provided default value.
    /// </summary>
    protected virtual int ReadRetryConfig(string key, int defaultValue)
    {
        var configured = _configuration?[key];
        if (!string.IsNullOrWhiteSpace(configured)
            && int.TryParse(configured, out int value)
            && value > 0)
        {
            return value;
        }

        return defaultValue;
    }

    public override async Task<IResponse<TKey>> Add(TNewVM model, CancellationToken ct = default)
    {
        IResponse<TKey> result = new Response<TKey>(ServiceName, OperationType.Add, _logger);

        await result.FillAsync(async () =>
        {
            await RetryHelper.RetryUntilAsync(SecondsBetweenRetries, MaxRetries, async () =>
            {
                var entityModel = _mapper.Map<TNewVM, TEntity>(model);
                await _repository.AddAsync(entityModel);
                result.Set(StatusCode.Succeeded, entityModel.Id);

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
            await RetryHelper.RetryUntilAsync(SecondsBetweenRetries, MaxRetries, async () =>
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

                return result.IsSuccess;
            }, ct);
        });

        return result;
    }

    public override async Task<IResponse<bool>> Delete(TKey id, CancellationToken ct = default)
    {
        IResponse<bool> result = new Response<bool>(ServiceName, OperationType.Delete, _logger);

        await result.FillAsync(async () =>
        {
            await RetryHelper.RetryUntilAsync(SecondsBetweenRetries, MaxRetries, async () =>
            {
                await _repository.DeleteAsync(id);

                result.Set(StatusCode.Succeeded, true);

                return result.IsSuccess;
            }, ct);
        });

        return result;
    }
}
