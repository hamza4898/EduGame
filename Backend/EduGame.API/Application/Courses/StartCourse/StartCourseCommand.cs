using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Courses.StartCourse;

public record StartCourseCommand(string UserId, int CourseId) : IRequest<Result>;