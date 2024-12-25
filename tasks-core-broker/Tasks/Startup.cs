using TaskExecutor.Services;

namespace TaskExecutor
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Добавление DatabaseService
            services.AddScoped<DatabaseServiceClient>();

            // Добавление TaskExecutionService
            services.AddScoped<TaskExecutionService>();

            // Добавление контроллера
            services.AddControllers();

            services.AddPrometheusCounters();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapMetrics();
            });
        }
    }
}