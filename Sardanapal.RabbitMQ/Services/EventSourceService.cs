
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Sardanapal.Contract.IModel;
using Sardanapal.Contract.IService;
using Sardanapal.Localization;
using Sardanapal.Share.EventArgModels;
using Sardanapal.ViewModel.Response;

namespace Sardanapal.RMQ.Services;

public abstract class EventSourceService<TKey, TModel> : IEventSourceService<TKey, TModel>, IDisposable
    where TKey : IEquatable<TKey>, IComparable<TKey>
    where TModel : IBaseEntityModel<TKey>, new()
{
    protected readonly IConnection ampqConnection;
    protected readonly ILogger _logger;
    protected abstract string exchangeName { get; set; }
    protected abstract string serviceName { get; set; }

    public EventSourceService(IConnection conn, ILogger logger)
    {
        ampqConnection = conn;
        this._logger = logger;
    }

    protected virtual async void Init()
    {
        using IChannel channel = await ampqConnection.CreateChannelAsync();
        await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Direct);
    }

    protected virtual Task<string> GetQueueName(string suffixName)
        => Task.FromResult(string.Concat(typeof(TModel).AssemblyQualifiedName, ".", suffixName));

    protected virtual Task<TModel> CreateModel(TModel model)
    {
        return Task.FromResult(model);
    }

    public async Task<IResponse<TKey>> Enqueue(OperationType queue, TModel model, CancellationToken ct = default)
    {
        IResponse<TKey> result = new Response<TKey>(serviceName, OperationType.Add, _logger);
        
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        result = await result.FillAsync(async () =>
        {
            using IChannel channel = await ampqConnection.CreateChannelAsync();

            model = await CreateModel(model);
            string jsonModel = JsonSerializer.Serialize(model);
            var body = new ReadOnlyMemory<byte>(Encoding.Default.GetBytes(jsonModel));
            await channel.BasicPublishAsync(exchangeName, await GetQueueName(queue.ToString()), body);

            result.Set(StatusCode.Succeeded, model.Id);
        });
        
        return result;
    }

    public async Task<IResponse<bool>> RegisterTopic(OperationType queueName, ESHandleEvent<TKey, TModel> handler, CancellationToken ct = default)
    {
        IResponse<bool> result = new Response<bool>(serviceName, OperationType.Function, _logger);

        result = await result.FillAsync(async () =>
        {
            using var channel = await ampqConnection.CreateChannelAsync();
            AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += ConsumeMessage(handler);
            await channel.BasicConsumeAsync(await GetQueueName(queueName.ToString()), false, consumer);
            result.Set(StatusCode.Succeeded, true);
        });

        return result;
    }

    protected abstract TKey NewId(TModel model);

    protected AsyncEventHandler<BasicDeliverEventArgs> ConsumeMessage(ESHandleEvent<TKey, TModel> handler)
    {
        return async (ch, ea) =>
        {
            var jsonBody = Encoding.Default.GetString(ea.Body.ToArray());
            var model = JsonSerializer.Deserialize<TModel>(jsonBody);
            handler(ch, new EventSourceEventArgs<TKey, TModel>()
            {
                Id = NewId(model),
                Model = model
            });

            await (ch as IChannel).BasicAckAsync(ea.DeliveryTag, true);
            _logger.LogInformation(ResourceHelper.CraeteRabbitMQMessageHandled(model.Id.ToString(), DateTime.UtcNow.ToString("yyyy-MM-dd | HH:mm")));
        };
    }

    public async void Dispose()
    {
        await ampqConnection.CloseAsync();
        ampqConnection.Dispose();
    }
}
