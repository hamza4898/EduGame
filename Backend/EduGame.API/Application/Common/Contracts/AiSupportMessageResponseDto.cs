namespace EduGame.Application.Common.Contracts;

public sealed record AiSupportMessageResponseDto(
    long Id,
    string Role,
    string Content,
    DateTimeOffset CreatedAt);