namespace EduGame.Application.Courses.UpdateCourseContent;

public record UpdateCourseLessonDto(
    int Id,
    string Title,
    int XpReward,
    List<UpdateCourseBlockDto> Blocks
);