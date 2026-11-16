namespace EduGame.Application.Courses.CreateCourseWithCommunity;

public record CourseCreationPathDto(int Id, string Name, List<CourseCreationSubSectionDto> SubSections);