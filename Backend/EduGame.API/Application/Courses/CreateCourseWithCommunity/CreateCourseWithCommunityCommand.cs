using Ardalis.Result;
using MediatR;

namespace EduGame.Application.Courses.CreateCourseWithCommunity;

public record CreateCourseWithCommunityCommand(CreateCourseWithCommunityDto Dto, string TeacherId, string TeacherName) : IRequest<Result<CourseWithCommunityCreatedDto>>;