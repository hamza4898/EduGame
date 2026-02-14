using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EduGame.Entities;

namespace EduGame.Data
{
    public class EduGameIdentityContext : IdentityDbContext<ApplicationUser>
    {
        public EduGameIdentityContext(DbContextOptions<EduGameIdentityContext> options) : base(options) {}
    }
}