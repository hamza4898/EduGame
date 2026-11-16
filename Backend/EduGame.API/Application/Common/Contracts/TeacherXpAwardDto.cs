using EduGame.Domain.Enums;

namespace EduGame.Application.Common.Contracts;

public sealed record TeacherXpAwardDto(
    string Reason,
    int CourseId,
    string CourseTitle,
    CourseDifficulty Difficulty,
    int Xp,
    DateTimeOffset CreatedAt);