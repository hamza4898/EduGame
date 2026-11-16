namespace EduGame.Application.Common.Contracts;

public sealed record TeacherRatingDto(
    string UserId,
    string UserName,
    string? AvatarUrl,
    int PublishedCoursesCount,
    int StudentsCount,
    double AverageProgress,
    double AverageRating,
    double Score);