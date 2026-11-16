namespace EduGame.Application.Common.Contracts;

public record CourseContentLessonDto(
    int Id,
    string Title,
    int Order,
    int XpReward,
    List<CourseContentBlockDto> Blocks
);