namespace EduGame.Application.Common.Contracts;

public record CourseContentSectionDto(
    int Id,
    string Title,
    int Order,
    List<CourseContentLessonDto> Lessons
);