using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Courses.GetCourse;

public record GetCourseByIdQuery(int CourseId, string StudentId) : IRequest<Result<CourseDetailDto>>;