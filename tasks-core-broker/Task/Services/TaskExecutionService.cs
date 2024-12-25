using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Shared.Models;
using TaskStatus = Shared.Enums.TaskStatus;

namespace TaskExecutor.Services
{
    public class TaskExecutionService
    {
        private readonly TaskProcessor _taskProcessor;
        private readonly IRabbitMqService _rabbitMqService;

        private const string _taskQueue = "taskQueue";
        private const string _resultQueue = "resultQueue";

        public TaskExecutionService(TaskProcessor taskProcessor, IRabbitMqService rabbitMqService)
        {
            _taskProcessor = taskProcessor;
            _rabbitMqService = rabbitMqService;
        }

        public async Task StartAsync()
        {
            // Ensure task and result queues are declared
            var channel = await _rabbitMqService.GetChannelAsync();
            
            // Subscribe to the task queue
            await _rabbitMqService.SubscribeToQueueAsync(_taskQueue, async message =>
            {
                var taskItem = JsonConvert.DeserializeObject<TaskItem>(message);

                if (taskItem != null)
                {
                    Console.WriteLine($"Received task: {taskItem.Id}");
                    await ProcessTaskAsync(taskItem);
                }
            });

            Console.WriteLine("TaskExecutionService is running...");
        }

        private async Task ProcessTaskAsync(TaskItem taskItem)
        {
            var cts = new CancellationTokenSource(taskItem.Ttl);

            try
            {
                // Execute task with a timeout
                double result = await _taskProcessor.ProcessAsync(taskItem, cts.Token);

                // Publish the result to the result queue
                await PublishResultAsync(new TaskResult
                {
                    TaskId = taskItem.Id,
                    Status = TaskStatus.Completed,
                    Result = result.ToString("#0.##", CultureInfo.InvariantCulture)
                });
            }
            catch (OperationCanceledException)
            {
                // Handle TTL expiration
                await PublishResultAsync(new TaskResult
                {
                    TaskId = taskItem.Id,
                    Status = TaskStatus.Expired,
                    Result = "Task expired."
                });
            }
            catch (Exception ex)
            {
                // Handle task failure
                await PublishResultAsync(new TaskResult
                {
                    TaskId = taskItem.Id,
                    Status = TaskStatus.Failed,
                    Result = $"Error: {ex.Message}"
                });
            }
        }

        private async Task PublishResultAsync(TaskResult taskResult)
        {
            var resultMessage = JsonConvert.SerializeObject(taskResult);
            var body = Encoding.UTF8.GetBytes(resultMessage);

            var channel = await _rabbitMqService.GetChannelAsync();

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: _resultQueue,
                body: body
            );

            Console.WriteLine($"Result published for Task {taskResult.TaskId}: {taskResult.Status}");
        }
    }
}
