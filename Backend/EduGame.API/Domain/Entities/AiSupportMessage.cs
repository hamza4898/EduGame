namespace EduGame.Domain.Entities;

public class AiSupportMessage
{
    public long Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}