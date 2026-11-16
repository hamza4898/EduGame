namespace EduGame.Application.Common.Contracts;

public sealed record UserSearchResponseDto(string UserId, string UserName, string? AvatarUrl, bool IsFriend, bool RequestPending);