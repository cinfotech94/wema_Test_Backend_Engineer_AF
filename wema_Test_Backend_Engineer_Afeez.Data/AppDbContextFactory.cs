using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System.IO;


namespace wema_Test_Backend_Engineer_Afeez.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            var connectionString = configuration.GetSection("Sql:ConnectionString").Value;

            optionsBuilder.UseSqlServer(connectionString);
            using var context = new AppDbContext(optionsBuilder.Options);
            context.Database.Migrate();
            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
