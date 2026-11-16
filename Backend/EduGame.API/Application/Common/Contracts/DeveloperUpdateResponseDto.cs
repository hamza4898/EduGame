namespace EduGame.Application.Common.Contracts;

public sealed record DeveloperUpdateResponseDto(
    int Id,
    string Title,
    string Category,
    string Content,
    string AuthorSignature,
    DateTimeOffset CreatedAt);