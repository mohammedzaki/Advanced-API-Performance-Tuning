using System.Text;
using RabbitMQ.Client;

namespace dotnet_sample.Services
{
    public class RabbitMqPublisher : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _exchangeName;
        private readonly string _routingKey;
        private readonly string? _queueName;

        public RabbitMqPublisher(string host, int port, string user, string pass, string exchangeName, string routingKey, string? queueName)
        {
            var factory = new ConnectionFactory
            {
                HostName = host,
                Port = port,
                UserName = user,
                Password = pass,
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(5),
                TopologyRecoveryEnabled = true
            };

            const int maxAttempts = 10;
            Exception? lastError = null;
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();
                    lastError = null;
                    break;
                }
                catch (Exception ex)
                {
                    lastError = ex;
                    if (attempt == maxAttempts)
                    {
                        throw;
                    }
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                }
            }
            _exchangeName = exchangeName;
            _routingKey = routingKey;
            _queueName = queueName;

            _channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Topic, durable: true, autoDelete: false);

            if (!string.IsNullOrWhiteSpace(_queueName))
            {
                _channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                _channel.QueueBind(queue: _queueName, exchange: _exchangeName, routingKey: _routingKey);
            }
        }

        public void Publish(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            var props = _channel.CreateBasicProperties();
            props.Persistent = true;
            _channel.BasicPublish(exchange: _exchangeName, routingKey: _routingKey, basicProperties: props, body: body);
        }

        public void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}