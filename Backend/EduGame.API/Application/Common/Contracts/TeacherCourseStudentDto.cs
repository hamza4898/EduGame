namespace EduGame.Application.Common.Contracts;

public sealed record TeacherCourseStudentDto(
    string StudentId,
    string StudentName,
    string? AvatarUrl,
    int CourseId,
    string CourseTitle,
    int ProgressPercent,
    bool IsCompleted,
    DateTimeOffset EnrolledAt);