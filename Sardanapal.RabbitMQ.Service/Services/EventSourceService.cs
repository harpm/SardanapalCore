using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Sardanapal.Contract.IService;
using Sardanapal.Share.EventArgModels;
using Sardanapal.ViewModel.Response;
using System.Text;
using System.Text.Json;

namespace Sardanapal.RabbitMQ.Service.Services;

public abstract class EventSourceService<TKey, TModel> : IEventSourceService<TKey, TModel>, IDisposable
    where TKey : IEquatable<TKey>, IComparable<TKey>
    where TModel : new()
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

    protected virtual async Task<string> GetQueueName(string suffixName) => string.Concat(typeof(TModel).AssemblyQualifiedName, ".", suffixName);

    protected virtual async Task<TModel> CreateModel(TModel model)
    {
        return model;
    }

    public async Task<IResponse<TModel>> Dequeue(OperationType queue)
    {
        IResponse<TModel> result = new Response<TModel>(serviceName, OperationType.Fetch);
        using IChannel channel = await ampqConnection.CreateChannelAsync();

        var message = await channel.BasicGetAsync(await GetQueueName(queue.ToString()), false);

        if (message == null)
        {
            result.Set(StatusCode.NotExists);
            return result;
        }
        else
        {
            string strBody = Encoding.Default.GetString(message.Body.ToArray());
            var model = JsonSerializer.Deserialize<TModel>(strBody);
            if (model != null)
            {
                result.Set(StatusCode.Succeeded, model);
            }
            else
            {
                result.Set(StatusCode.Failed);
            }

        }
        return result;
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

            // TODO: fill the result properly
            result.Set(StatusCode.Succeeded);
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

    protected AsyncEventHandler<BasicDeliverEventArgs> ConsumeMessage(ESHandleEvent<TKey, TModel> handler)
    {
        return async (ch, ea) =>
        {
            var jsonBody = Encoding.Default.GetString(ea.Body.ToArray());
            var model = JsonSerializer.Deserialize<TModel>(jsonBody);
            handler(ch, new EventSourceEventArgs<TKey, TModel>()
            {
                // TODO: assign real id
                Id = default(TKey),
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