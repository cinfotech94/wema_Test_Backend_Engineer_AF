using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using StackExchange.Redis;
using System;
using System.IO;
using wema_Test_Backend_Engineer_Afeez.Data;
using wema_Test_Backend_Engineer_Afeez.Data.Repository;
using wema_Test_Backend_Engineer_Afeez.Services.Implementation;
using wema_Test_Backend_Engineer_Afeez.Services.Interface;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configuration setup
        var configuration = builder.Configuration;

        // Register DbContext with the SQL Server provider

        builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        // Register Repo<TEntity> for each TEntity you have in your application
        
        builder.Services.AddScoped<IRepo, Repo>();
        builder.Services.AddScoped<AppDbContext>();
        // Configure Redis
        var redisConnectionString = configuration.GetSection("Redis:ConnectionString").Value;
        builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog());

        // Register other services
        builder.Services.AddScoped<ICacheService, CacheService>();
        builder.Services.AddScoped<ILogService, LogService>();
        builder.Services.AddScoped<IWemaTestMain, WemaTestMain>();

        // Add controllers
        builder.Services.AddControllers();

        // Add Swagger for API documentation
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Wema Test By Afeez", Version = "v1" });
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}
