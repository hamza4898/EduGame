namespace EduGame.Application.Courses.UpdateCourseContent;

public record UpdateCourseSectionDto(
    int Id,
    string Title,
    List<UpdateCourseLessonDto> Lessons
);