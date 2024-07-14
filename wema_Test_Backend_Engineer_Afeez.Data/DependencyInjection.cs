using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using wema_Test_Backend_Engineer_Afeez.Data.Repository;

namespace wema_Test_Backend_Engineer_Afeez.Data
{
    public static class DependencyInjection
    {
        public static void AddDataServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetSection("Sql:ConnectionString").Value;
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddScoped<IRepo, Repo>(); // Register your repository
        }
    }
}
