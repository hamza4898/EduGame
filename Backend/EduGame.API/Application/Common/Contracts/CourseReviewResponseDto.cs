namespace EduGame.Application.Common.Contracts;
public sealed record CourseReviewResponseDto(
    int Id,
    string StudentId,
    string Student,
    string? AvatarUrl,
    int Stars,
    string Text,
    DateTimeOffset CreatedAt);