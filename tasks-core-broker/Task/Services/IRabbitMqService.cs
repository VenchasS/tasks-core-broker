using RabbitMQ.Client;

namespace TaskExecutor.Services
{
    public interface IRabbitMqService
    {
        Task<IChannel> GetChannelAsync();
        
        Task SubscribeToQueueAsync(string queueName, Func<string, Task> messageHandler);

        ValueTask DisposeAsync();
    }
}