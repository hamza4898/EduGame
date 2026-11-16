namespace EduGame.Domain.Entities;

public class PasswordResetCode
{
    public long Id { get; set; }

    public Guid ExternalId { get; set; } = Guid.NewGuid();

    public string HashedCode { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public int Attempts { get; set; } = 0;

    public bool IsUsed { get; set; } = false;

    public bool IsVerified { get; set; } = false;

    public DateTime? VerifiedAt { get; set; }
    
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = default!;

    public bool IsActive => !IsUsed && Attempts <= 5 && ExpiresAt > DateTime.UtcNow;
}