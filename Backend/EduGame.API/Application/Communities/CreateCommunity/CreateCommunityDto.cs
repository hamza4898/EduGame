using EduGame.Domain.Enums;

namespace EduGame.Application.Communities.CreateCommunity;

public record CreateCommunityDto(
    string CommunityName, 
    string? Description,
    CommunityPrivacyType PrivacyType,
    CommunityMembersLimit MembersLimit
);