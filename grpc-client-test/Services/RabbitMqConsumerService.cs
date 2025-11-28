using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace GrpcClientTest.Services;

public class RabbitMqConsumerService : BackgroundService
{
    private readonly ILogger<RabbitMqConsumerService> _logger;
    private readonly string _host;
    private readonly int _port;
    private readonly string _user;
    private readonly string _pass;
    private readonly string _exchange;
    private readonly string _routingKey;
    private readonly string _queue;

    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMqConsumerService(
        ILogger<RabbitMqConsumerService> logger,
        string host,
        int port,
        string user,
        string pass,
        string exchange,
        string routingKey,
        string queue)
    {
        _logger = logger;
        _host = host;
        _port = port;
        _user = user;
        _pass = pass;
        _exchange = exchange;
        _routingKey = routingKey;
        _queue = queue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _host,
            Port = _port,
            UserName = _user,
            Password = _pass,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(5),
            DispatchConsumersAsync = true
        };

        // Retry to establish connection
        for (var attempt = 1; attempt <= 20 && !stoppingToken.IsCancellationRequested; attempt++)
        {
            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "RabbitMQ connection attempt {Attempt} failed; retrying...", attempt);
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }

        if (_channel is null)
        {
            _logger.LogError("RabbitMQ channel not created; consumer will not start.");
            return;
        }

        // Ensure topology exists (idempotent)
        _channel.ExchangeDeclare(_exchange, ExchangeType.Topic, durable: true, autoDelete: false);
        _channel.QueueDeclare(_queue, durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueBind(_queue, _exchange, _routingKey);
        _channel.BasicQos(0, 10, false); // prefetch 10

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                _logger.LogInformation("[RabbitMQ] Received message on {Queue}: {Message}", _queue, message);
                _channel.BasicAck(ea.DeliveryTag, multiple: false);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RabbitMQ] Error processing message; nacking");
                _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        _channel.BasicConsume(queue: _queue, autoAck: false, consumer: consumer);
        _logger.LogInformation("[RabbitMQ] Consumer started on queue {Queue}", _queue);

        // Keep running until cancellation
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    public override void Dispose()
    {
        try { _channel?.Close(); _channel?.Dispose(); } catch { }
        try { _connection?.Close(); _connection?.Dispose(); } catch { }
        base.Dispose();
    }
}