namespace EduGame.Application.Chat.GetMyCommunities;

public class MyCommunitiesResponseDto
{
    public Guid CommunityId { get; set; }

    public string CommunityName { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public string? Description { get; set; }
    
    public int MembersCount { get; set; }
}