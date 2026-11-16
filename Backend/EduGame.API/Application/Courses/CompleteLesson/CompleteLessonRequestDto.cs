namespace EduGame.Application.Courses.CompleteLesson;

public sealed record CompleteLessonRequest(
    int LessonId, 
    int? SelectedAnswer);