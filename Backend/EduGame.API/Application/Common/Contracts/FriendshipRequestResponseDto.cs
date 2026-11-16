namespace EduGame.Application.Common.Contracts;

public record FriendshipRequestResponseDto(
    int RequestId,
    string UserId,
    string UserName,
    string? AvatarUrl,
    DateTime CreatedAt
);