namespace EduGame.Application.Chat.SearchCommunities;

public record SearchCommunitiesResponseDto(
    Guid Id,
    string Name,
    int MembersCount,
    string AvatarUrl,
    string? Description
);