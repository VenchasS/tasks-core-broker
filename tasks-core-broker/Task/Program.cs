
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using TaskExecutor.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Enums;
using Shared.Models;



namespace TaskExecutor
{
    public class TaskProcessor
    {
        public async Task<double> ProcessAsync(TaskItem task, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Processing Task {task.Id}...");

            try
            {
                var result = Calculate(task.Type, task.Data);
                var random = new Random();

                var (minDelay, range) = GetDelayRange(task.Ttl);
                var simulatedDelay = random.NextDouble() * range + minDelay;
                var totalDelay = TimeSpan.FromMilliseconds(simulatedDelay);

                await SimulateProcessingAsync(totalDelay, cancellationToken);

                Console.WriteLine($"Task {task.Id} completed with result: {result}");
                return result;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"Task {task.Id} was canceled.");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing Task {task.Id}: {ex.Message}");
                throw;
            }
        }

        private static (double, double) GetDelayRange(double ttl)
        {
            var minDelay = Math.Max(10, ttl * 0.1);
            var maxDelay = ttl * 1.1;
            return (minDelay, maxDelay - minDelay);
        }

        private static async Task SimulateProcessingAsync(TimeSpan totalDelay, CancellationToken cancellationToken)
        {
            var startTime = DateTime.UtcNow;

            while (DateTime.UtcNow - startTime < totalDelay)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(10, cancellationToken);
            }
        }

        private static double Calculate(TaskType taskType, string data)
        {
            var operands = ParseOperands(data);

            return taskType switch
            {
                TaskType.Addition => operands.left + operands.right,
                TaskType.Subtraction => operands.left - operands.right,
                TaskType.Multiplication => operands.left * operands.right,
                TaskType.Division when operands.right != 0 => operands.left / operands.right,
                TaskType.Division => throw new DivideByZeroException("Division by zero"),
                _ => throw new InvalidOperationException($"Unknown task type: {taskType}")
            };
        }

        private static (double left, double right) ParseOperands(string data)
        {
            var parts = data.Split(',');

            if (parts.Length != 2)
                throw new InvalidOperationException("Invalid task format");

            return (double.Parse(parts[0]), double.Parse(parts[1]));
        }
    }

    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly TaskExecutionService _taskExecutionService;

        public Worker(ILogger<Worker> logger, TaskExecutionService taskExecutionService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _taskExecutionService = taskExecutionService ?? throw new ArgumentNullException(nameof(taskExecutionService));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await _taskExecutionService.StartAsync();
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IRabbitMqService, RabbitMqService>();

            services.AddSingleton<TaskProcessor>();

            services.AddSingleton<TaskExecutionService>();

            services.AddHostedService<Worker>();

            services.AddSingleton(Configuration);
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            BuildHost(args).Run();
        }

        private static IHost BuildHost(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;
                    var envName = env.EnvironmentName;
                    Console.WriteLine($"Environment: {envName}");
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                 .AddJsonFile($"appsettings.{envName}.json", optional: true, reloadOnChange: true)
                                 .AddEnvironmentVariables();
                })
                .ConfigureServices((context, services) =>
                {
                    new Startup(context.Configuration).ConfigureServices(services);
                })
                .Build();
    }
}
