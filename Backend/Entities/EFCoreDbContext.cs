using Microsoft.EntityFrameworkCore;

namespace EduGame.Entities
{
    public class EFCoreDbContext : DbContext
    {
        public EFCoreDbContext() { }

        public EFCoreDbContext(DbContextOptions<EFCoreDbContext> options) : base(options) { }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build()
                    .GetConnectionString("DefaultConnection");
                
                optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(9, 0, 5)));
            }
        }

        public DbSet<Student> Students { get; set; }

        public DbSet<Teacher> Teachers { get; set; }

        public DbSet<Partner> Partners { get; set; }
    }
}