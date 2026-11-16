using EduGame.Domain.Enums;

namespace EduGame.Domain.Entities;

public class Message
{
    public string MessageId { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;

    public string MessageType { get; set; } = string.Empty;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public string SenderId { get; set; } = string.Empty;

    public MessageStatus Status { get; set; }

    public int DeliveredCount { get; set; } = 1;

    public int ReadCount { get; set; } = 1;

    public Community? Community { get; set; } = null!;

    public int? CommunityId { get; set; }
}