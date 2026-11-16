namespace EduGame.Application.Chat.SearchCommunities;

public record SearchCommunitiesDto(
    string Query,
    int Offset,
    int Limit
);