using Microsoft.EntityFrameworkCore;
using EduGame.Entities;

namespace EduGame.Data
{
    public class EduGameDbContext : DbContext
    {
        public EduGameDbContext() { }

        public EduGameDbContext(DbContextOptions<EduGameDbContext> options) : base(options) { }
        
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