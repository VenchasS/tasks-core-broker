
namespace TaskExecutor.Services
{
    public class TaskExecutionService : BackgroundService
    {
        private readonly ILogger<TaskExecutionService> _logger;
        private readonly DatabaseServiceClient _databaseServiceClient;

        public TaskExecutionService(ILogger<TaskExecutionService> logger, DatabaseServiceClient databaseServiceClient)
        {
            _logger = logger;
            _databaseServiceClient = databaseServiceClient;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }, stoppingToken);

            return Task.CompletedTask;
        }

        private async Task ProcessTask(TaskModel taskItem)
        {
            try
            {
                _logger.LogInformation($"Processing task {taskItem.Id}: {taskItem.Description}");

                var taskResult = new Models.Models
                {
                    TaskId = taskItem.Id,
                    Result = "Task completed successfully",
                    CompletedAt = DateTime.UtcNow
                };

                await _databaseServiceClient.SaveTaskResultAsync(taskResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing task {TaskId}", taskItem.Id);
            }
        }
    }
}
