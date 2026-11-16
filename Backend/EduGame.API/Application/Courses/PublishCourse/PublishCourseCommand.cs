using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Courses.PublishCourse;

public sealed record PublishCourseCommand(string UserId, int CourseId) : IRequest<Result<CourseReadinessResponseDto>>;