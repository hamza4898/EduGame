namespace EduGame.Application.Common.Contracts;

public sealed record FriendResponseDto(int RequestId, string UserId, string UserName, string? AvatarUrl);