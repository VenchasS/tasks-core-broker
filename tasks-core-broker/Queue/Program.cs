using TaskQueue.Database;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using TaskQueue.Repositories;
using TaskQueue.Services;

namespace TaskQueue
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddSingleton<IRabbitMqService, BrokerService>();
            services.AddScoped<TaskRepository>();
            services.AddScoped<QueueService>();
            services.AddSingleton<IConfiguration>(Configuration);

            services.AddControllers();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "TaskQueue API", Version = "v1" });
                options.EnableAnnotations();
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskQueue API V1");
                c.RoutePrefix = string.Empty;
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            CheckDatabaseConnection(host);
            host.Run();
        }

        private static void CheckDatabaseConnection(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var context = services.GetRequiredService<AppDbContext>();
                context.Database.EnsureCreated();
                Console.WriteLine("Database connected successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while connecting to the database: {ex.Message}");
                Environment.Exit(1);
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;

                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                          .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                          .AddEnvironmentVariables();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
