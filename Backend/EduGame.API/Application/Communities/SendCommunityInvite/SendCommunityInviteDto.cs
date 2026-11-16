namespace EduGame.Application.Communities.SendCommunityInvite;

public record InviteFriendDto(string ReceiverId, Guid CommunityId, string CommunityName);