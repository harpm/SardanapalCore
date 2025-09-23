using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using Sardanapal.Contract.IService;
using Sardanapal.Share.EventArgModels;
using Sardanapal.Localization;

namespace Sardanapal.RMQ.Services;


public class EventBusRabbitMQ : ISardanapalEventBus
{
    protected readonly ILogger _logger;
    protected readonly IRabbitMQPersistentConnection _persistentConnection;
    protected virtual string _exchangeName => "event_bus";

    public EventBusRabbitMQ(IRabbitMQPersistentConnection persistentConnection, ILogger<EventBusRabbitMQ> logger)
    {
        _persistentConnection = persistentConnection;
        _logger = logger;

        if (!_persistentConnection.IsConnected)
            _persistentConnection.TryConnect();
        Task.Run(async () =>
        {
            using var channel = await _persistentConnection.CreateModel();
            await channel.ExchangeDeclareAsync(exchange: _exchangeName, type: ExchangeType.Topic, durable: true);
        });
    }

    public async Task Publish(IntegrationEvent e)
    {
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
        var channel = await _persistentConnection.CreateModel();
        var eventName = typeof(T).Name;
        var queueName = eventName + $"_{eventName}";

        await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        await channel.QueueBindAsync(queue: queueName, exchange: _exchangeName, routingKey: eventName);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());
            var e = JsonSerializer.Deserialize<T>(message);

            var handler = new TH();
            if (handler != null && e != null)
                await handler.Handle(e);

            await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
            _logger.LogInformation(ResourceHelper.CraeteRabbitMQMessageHandled(e.Id.ToString(), e.CreationDate.ToString("yyyy-MM-dd | HH:mm")));
        };

        await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);
    }
}
