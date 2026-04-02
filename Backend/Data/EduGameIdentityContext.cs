using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EduGame.Entities;

namespace EduGame.Data
{
    public class EduGameIdentityContext : IdentityDbContext<ApplicationUser>
    {
        public EduGameIdentityContext(DbContextOptions<EduGameIdentityContext> options) : base(options) {}

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>()
                .Property(x => x.RegisteredAt)
                .HasColumnType("datetime")
                .IsRequired();
        }

        public DbSet<RefreshToken> RefreshTokens { get; set; }
    }
}