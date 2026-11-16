namespace EduGame.Application.Courses.CreateCourseWithCommunity;

public record CourseWithCommunityCreatedDto(int CourseId, Guid CommunityId, string CommunityName);