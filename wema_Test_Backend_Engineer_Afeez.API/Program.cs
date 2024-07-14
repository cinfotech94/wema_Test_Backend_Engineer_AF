using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.IO;
using wema_Test_Backend_Engineer_Afeez.Data;
using wema_Test_Backend_Engineer_Afeez.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuration setup
var configuration = builder.Configuration;

// Register DbContext with the SQL Server provider
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(configuration.GetSection("Sql:ConnectionString").Value));

// Ensure the logs directory exists
if (!Directory.Exists("logs"))
{
    Directory.CreateDirectory("logs");
}

// Add data services (repository)
builder.Services.AddDataServices(configuration);

// Add additional application services as needed
builder.Services.AddApplicationServices(configuration);

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
