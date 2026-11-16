namespace EduGame.Application.Courses.GetMyTeacherCourseProgress;

public sealed record TeacherCourseProgressDto(
    int CourseId,
    double AverageProgress);