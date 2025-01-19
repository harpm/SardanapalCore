using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Sardanapal.Contract.IModel;
using Sardanapal.Contract.IService;
using Sardanapal.Share.EventArgModels;
using Sardanapal.ViewModel.Response;
using System.Text;
using System.Text.Json;

namespace Sardanapal.RabbitMQ.Service.Services;

public abstract class EventSourceService<TKey, TModel> : IEventSourceService<TKey, TModel>, IDisposable
    where TKey : IEquatable<TKey>, IComparable<TKey>
    where TModel : IBaseEntityModel<TKey>, new()
{
    protected IConnection ampqConnection { get; set; }
    protected abstract string exchangeName { get; set; }
    protected abstract string serviceName { get; set; }

    public EventSourceService(IConnection conn)
    {
        ampqConnection = conn;
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

    public async Task<IResponse<TKey>> Enqueue(OperationType queue, TModel model)
    {
        IResponse<TKey> result = new Response<TKey>(serviceName, OperationType.Add);
        
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

    public async Task<IResponse<bool>> RegisterTopic(OperationType queueName, ESHandleEvent<TKey, TModel> handler)
    {
        IResponse<bool> result = new Response<bool>(serviceName, OperationType.Function);

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
        };
    }

    public async void Dispose()
    {
        await ampqConnection.CloseAsync();
        ampqConnection.Dispose();
    }
}