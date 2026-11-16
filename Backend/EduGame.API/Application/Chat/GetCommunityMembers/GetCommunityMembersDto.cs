namespace EduGame.Application.Chat.GetCommunityMembers;

public record GetCommunityMembersDto(Guid CommunityId, int Offset, int Limit);