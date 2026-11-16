using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Courses.ToggleArchivedCourse;

public record ToggleArchivedCourseCommand(string UserId, int CourseId) : IRequest<Result<bool>>;