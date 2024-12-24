using RabbitMQ.Client;

namespace TaskQueue.Services
{
    public interface IRabbitMqService
    {
        Task<IChannel> GetChannelAsync();
    }

    public class BrokerService : IRabbitMqService
    {
        private readonly ConnectionFactory _connectionFactory;
        private IConnection? _connection;
        private IChannel? _channel;

        public BrokerService(IConfiguration configuration)
        {
            _connectionFactory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:Host"],
                Port = configuration.GetValue<int>("RabbitMQ:Port"),
                UserName = configuration["RabbitMQ:UserName"],
                Password = configuration["RabbitMQ:Password"]
            };
        }

        public async Task<IChannel> GetChannelAsync()
        {
            if (_connection == null || !_connection.IsOpen)
            {
                _connection = await _connectionFactory.CreateConnectionAsync();
            }
            if (_channel == null || !_channel.IsOpen)
            {
                _channel = await _connection.CreateChannelAsync();
                await DeclareQueueAsync(_channel);
            }
            return _channel;
        }
        
        private async Task DeclareQueueAsync(IChannel channel)
        {
            await channel.QueueDeclareAsync(
                queue: "taskQueue",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
        }
    }
}