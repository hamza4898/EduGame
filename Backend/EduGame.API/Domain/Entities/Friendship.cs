using EduGame.Domain.Enums;

namespace EduGame.Domain.Entities;

public class Friendship
{
    public int Id { get; set; }

    public string SenderId { get; set; } = string.Empty;

    public BaseUser Sender { get; set; } = null!;

    public string ReceiverId { get; set; } = string.Empty;

    public BaseUser Receiver { get; set; } = null!;

    public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}