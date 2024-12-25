using RabbitMQ.Client;
using Newtonsoft.Json;
using System.Text;
using Prometheus;

namespace TaskExecutor
{
    public class TaskProcessor : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TaskProcessor> _logger;
        private static readonly Counter TaskProcessedSuccessfully = Metrics.CreateCounter("task_processed_successfully", "Number of tasks processed successfully");
        private static readonly Counter TaskProcessingFailed = Metrics.CreateCounter("task_processing_failed", "Number of tasks that failed to process");

        public TaskProcessor(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<TaskProcessor> logger)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;

            // Настройки RabbitMQ из конфигурации
            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQ:Host"],
                UserName = _configuration["RabbitMQ:Username"],
                Password = _configuration["RabbitMQ:Password"]
            };
            
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: _configuration["RabbitMQ:QueueName"],
                                  durable: true,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);

            _channel.BasicQos(0, 1, false);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var task = JsonConvert.DeserializeObject<TaskMessage>(message);

                // Выполнение задачи
                var result = await ProcessTask(task);

                // Отправка результата в TaskQueue
                await SendResultToTaskQueue(task.Id, result);

                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            _channel.BasicConsume(queue: _configuration["RabbitMQ:QueueName"],
                                  autoAck: false,
                                  consumer: consumer);

            return Task.CompletedTask;
        }

        // Пример выполнения задачи
        private async Task<string> ProcessTask(TaskMessage task)
        {
            // Здесь идет выполнение задачи
            await Task.Delay(1000); // Задержка для имитации
            // Инкрементируем счетчик успешных задач
            TaskProcessedSuccessfully.Inc();
            return "Success";
        }

        // Отправка результата в TaskQueue через HTTP API
        private async Task SendResultToTaskQueue(string taskId, string status,  string result)
        {
            var httpClient = _httpClientFactory.CreateClient();

            var taskResult = new Models.Models
            {
                Id = taskId,
                Status = status,
                Result = result
            };

            var content = new StringContent(JsonConvert.SerializeObject(taskResult), Encoding.UTF8, "application/json");
            var url = $"{_configuration["TaskQueueAPI:BaseUrl"]}{_configuration["TaskQueueAPI:ResultEndpoint"]}";

            var response = await httpClient.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Ошибка отправки результата задачи {TaskId} в TaskQueue. Статус: {Status}", taskId, response.StatusCode);
            }
        }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}
