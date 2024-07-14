using Microsoft.EntityFrameworkCore;
using wema_Test_Backend_Engineer_Afeez.Domain.Model;

namespace wema_Test_Backend_Engineer_Afeez.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Customer> Customers { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
