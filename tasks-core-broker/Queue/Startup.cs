using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using TaskQueue.Database;
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

            services.AddSingleton<IRabbitMqService, RabbitMqService>();
            services.AddScoped<TaskRepository>();
            services.AddScoped<TaskQueueService>();
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
}
