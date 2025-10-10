// Licensed under the MIT license.

using AutoMapper;
using Microsoft.Extensions.Logging;
using Sardanapal.Contract.IModel;
using Sardanapal.Contract.IRepository;
using Sardanapal.Contract.IService;
using Sardanapal.Service.Utilities;
using Sardanapal.ViewModel.Models;
using Sardanapal.ViewModel.Response;

namespace Sardanapal.Service;

public abstract class MemoryCrudServiceBase<TRepository, TModel, TKey, TSearchVM, TVM, TNewVM, TEditableVM>
    : ICrudService<TKey, TSearchVM, TVM, TNewVM, TEditableVM>
    where TRepository : class, IMemoryRepository<TKey, TModel>
    where TModel : class, IBaseEntityModel<TKey>, new()
    where TKey : IEquatable<TKey>, IComparable<TKey>
    where TSearchVM : class, new()
    where TVM : class, new()
    where TNewVM : class, new()
    where TEditableVM : class, new()

{
    protected abstract string ServiceName { get; }
    protected readonly IMapper _mapper;
    protected readonly TRepository _repository;
    protected readonly ILogger _logger;

    public MemoryCrudServiceBase(TRepository repository, IMapper mapper, ILogger logger)
    {
        this._repository = repository;
        this._mapper = mapper;
        this._logger = logger;
    }

    protected abstract IEnumerable<TModel> Search(IEnumerable<TModel> entities, TSearchVM searchVM);

    public async Task<IResponse<TKey>> Add(TNewVM model, CancellationToken ct = default)
    {
        IResponse<TKey> result = new Response<TKey>(ServiceName, OperationType.Add, _logger);

        await result.FillAsync(async () =>
        {
            var entity = _mapper.Map<TModel>(model);
            var newId = await _repository.AddAsync(entity, ct);
            result.Set(StatusCode.Succeeded, newId);
        });

        return result;
    }

    public async Task<IResponse<TVM>> Get(TKey id, CancellationToken ct = default)
    {
        IResponse<TVM> result = new Response<TVM>(ServiceName, OperationType.Fetch, _logger);

        await result.FillAsync(async () =>
        {
            var entity = await _repository.FetchByIdAsync(id, ct);
            var model = _mapper.Map<TVM>(entity);
            result.Set(StatusCode.Succeeded, model);
        });

        return result;
    }

    public async Task<IResponse<GridVM<TKey, T>>> GetAll<T>(GridSearchModelVM<TKey, TSearchVM> searchModel = null, CancellationToken ct = default) where T : class
    {
        IResponse<GridVM<TKey, T>> result = new Response<GridVM<TKey, T>>(ServiceName, OperationType.Fetch, _logger);

        await result.FillAsync(async () =>
        {
            if (searchModel is null)
                searchModel = new();
            if (searchModel.Fields is null)
                searchModel.Fields = new();

            var data = new GridVM<TKey, T>(searchModel);

            var entities = await _repository.FetchAllAsync(ct);
            entities = Search(entities, searchModel.Fields);

            data.SearchModel.TotalCount = entities.Count();

            var list = EnumerableHelper.Search(entities, searchModel).Select(x => _mapper.Map<T>(x)).ToList();
            data.List = list;

            result.Set(StatusCode.Succeeded, data);
        });

        return result;
    }

    public async Task<IResponse<TEditableVM>> GetEditable(TKey id, CancellationToken ct = default)
    {
        IResponse<TEditableVM> result = new Response<TEditableVM>(ServiceName, OperationType.Fetch, _logger);

        await result.FillAsync(async () =>
        {
            var entity = await _repository.FetchByIdAsync(id, ct);
            var model = _mapper.Map<TEditableVM>(entity);
            result.Set(StatusCode.Succeeded, model);
        });

        return result;
    }
    public async Task<IResponse<bool>> Edit(TKey id, TEditableVM model, CancellationToken ct = default)
    {
        IResponse<bool> result = new Response<bool>(ServiceName, OperationType.Edit, _logger);

        await result.FillAsync(async () =>
        {
            var entity = await _repository.FetchByIdAsync(id, ct);
            if (entity != null)
            {
                entity = _mapper.Map(model, entity);
                var isEditted = await _repository.UpdateAsync(id, entity);
                result.Set(StatusCode.Succeeded, isEditted);
            }
            else
            {
                result.Set(StatusCode.NotExists);
            }
        });

        return result;
    }

    public async Task<IResponse<bool>> Delete(TKey id, CancellationToken ct = default)
    {
        IResponse<bool> result = new Response<bool>(ServiceName, OperationType.Delete, _logger);

        await result.FillAsync(async () =>
        {
            var isDeleted = await _repository.DeleteAsync(id, ct);
            result.Set(isDeleted ? StatusCode.Succeeded : StatusCode.Failed, isDeleted);
        });

        return result;
    }
}
