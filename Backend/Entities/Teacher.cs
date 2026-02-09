using Microsoft.EntityFrameworkCore;

namespace EduGame.Entities
{
    [Index(nameof(ExternalId), IsUnique = true)]
    public class Teacher
    {
        public int Id { get; set; }

        public Guid ExternalId { get; set; } = Guid.NewGuid();

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Gender { get; set; }

        public string? Phone { get; set; }

        public string? Subject { get; set; }

        public string? Email { get; set; }

        public string? PasswordHash { get; set; }

        public string? Motivation { get; set; }

        public DateTime RegisteredAt { get; set; } = DateTime.Now;
    }
}