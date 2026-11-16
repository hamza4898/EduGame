namespace EduGame.Application.Courses.UpdateCourseContent;

public record UpdateCourseBlockDto(
    int Type,
    string Content,
    int Order
);