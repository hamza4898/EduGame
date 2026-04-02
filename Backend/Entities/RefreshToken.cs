using Microsoft.EntityFrameworkCore;
using EduGame.Entities;

namespace EduGame.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }

        public string HashedToken { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;

        public ApplicationUser User { get; set; } = default!;

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        public bool IsRevoked { get; set; }

        public DateTime? RevokedAt { get; set; }

        public bool IsUsed { get; set; }

        public bool IsActive => !IsRevoked && !IsUsed && DateTime.UtcNow < ExpiresAt;
    }
}