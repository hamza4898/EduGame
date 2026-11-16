namespace EduGame.Domain.Entities;

public class Notification
{
    public int Id { get; set; }

    public string ReceiverId { get; set; } = string.Empty;

    public string SenderId { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string Type { get; set; } = "CommunityInvite";

    public Guid? TargetId { get; set; }

    public int? CourseId { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}