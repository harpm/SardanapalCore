using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using Sardanapal.Contract.IService;
using Sardanapal.Share.EventArgModels;
using Sardanapal.Localization;

namespace Sardanapal.RMQ.Services;


public class EventBusRabbitMQ : ISardanapalEventBus, IDisposable
{
    protected readonly ILogger _logger;
    protected readonly IRabbitMQPersistentConnection _persistentConnection;
    protected virtual string _exchangeName => "event_bus";

    private readonly Task _initialization;
    private readonly List<IChannel> _consumerChannels = new();
    private bool _disposed;

    public EventBusRabbitMQ(IRabbitMQPersistentConnection persistentConnection, ILogger<EventBusRabbitMQ> logger)
    {
        _persistentConnection = persistentConnection;
        _logger = logger;

        _initialization = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        if (!_persistentConnection.IsConnected)
            await _persistentConnection.TryConnect();

        using var channel = await _persistentConnection.CreateModel();
        await channel.ExchangeDeclareAsync(exchange: _exchangeName, type: ExchangeType.Topic, durable: true);
    }

    public async Task Publish(IntegrationEvent e)
    {
        await _initialization;

        using var channel = await _persistentConnection.CreateModel();

        var routingKey = e.GetType().Name; // Use event type as routing key
        var message = JsonSerializer.Serialize(e);
        var body = Encoding.UTF8.GetBytes(message);

        var prop = new BasicProperties();
        prop.DeliveryMode = DeliveryModes.Persistent;

        await channel.BasicPublishAsync(exchange: _exchangeName,
            mandatory: true,
            basicProperties: prop,
            routingKey: routingKey,
            body: body);

        _logger.LogInformation(ResourceHelper.CraeteRabbitMQMessagePublished(e.Id.ToString(), e.CreationDate.ToString("yyyy-MM-dd | HH:mm")));
    }

    public async Task Subscribe<T, TH>(string eventType)
        where T : IntegrationEvent
        where TH : IIntegrationEventHandler<T>, new()
    {
        await _initialization;

        var channel = await _persistentConnection.CreateModel();
        _consumerChannels.Add(channel);

        var eventName = typeof(T).Name;
        var queueName = eventName + $"_{eventName}";

        await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        await channel.QueueBindAsync(queue: queueName, exchange: _exchangeName, routingKey: eventName);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                var e = JsonSerializer.Deserialize<T>(message);

                if (e != null)
                {
                    var handler = new TH();
                    await handler.Handle(e);
                }

                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                _logger.LogInformation(ResourceHelper.CraeteRabbitMQMessageHandled(e.Id.ToString(), e.CreationDate.ToString("yyyy-MM-dd | HH:mm")));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to handle message from queue '{QueueName}'. Nacking without requeue.", queueName);
                await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
            }
        };

        await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);
    }

    public void Dispose()
    {
        if (_disposed) return;

        foreach (var channel in _consumerChannels)
        {
            try { channel.Dispose(); }
            catch { }
        }
        _consumerChannels.Clear();
        _disposed = true;
    }
}
