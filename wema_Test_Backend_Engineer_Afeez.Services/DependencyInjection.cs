using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using StackExchange.Redis;
using wema_Test_Backend_Engineer_Afeez.Domain.DTO;
using wema_Test_Backend_Engineer_Afeez.Services.Implementation;
using wema_Test_Backend_Engineer_Afeez.Services.Interface;

namespace wema_Test_Backend_Engineer_Afeez.Services
{
    public static class DependencyInjection
    {
        public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure Redis
            var redisConnectionString = configuration.GetConnectionString("RedisConnection");
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));

            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog());

            // Register other services
            services.AddScoped<ICacheService, CacheService>();
            services.AddScoped<ILogService, LogService>();
            services.AddScoped<IWemaTestMain, WemaTestMain>();
        }
    }
}
