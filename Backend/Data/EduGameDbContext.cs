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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(EduGameDbContext).Assembly);
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder builder)
        {
            builder.Properties<string>()
                .HaveMaxLength(255)
                .AreUnicode(true);
        }

        public DbSet<Student> Students { get; set; }

        public DbSet<Teacher> Teachers { get; set; }

        public DbSet<Partner> Partners { get; set; }
    }
}