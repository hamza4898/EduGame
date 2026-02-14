using System.Text;
using Microsoft.AspNetCore.Identity;

namespace EduGame.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime RegisteredAt { get; set; } = DateTime.Now;
    }
}