using EduGame.Domain.Enums;

namespace EduGame.Domain.Entities;

public class CommunityMember
{
    public int Id { get; set; }

    public Guid CommunityId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public ClanRole Role { get; set; } = ClanRole.Member;

    public string UserName { get; set; } = string.Empty;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public string AvatarUrl { get; set; } = string.Empty;

    public Community Community { get; set; } = null!;
}