using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Courses.ToggleLikedCourse;

public record ToggleLikedCourseCommand(string UserId, int CourseId) : IRequest<Result<bool>>;