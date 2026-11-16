using EduGame.Domain.Enums;

namespace EduGame.Application.Chat.GetCommunityMembers;

public sealed record GetCommunityMembersResponseDto(
    string CommunityName,
    string UserName,
    ClanRole UserRole,
    string AvatarUrl,
    string UserId,
    bool IsFriend,
    bool RequestPending);