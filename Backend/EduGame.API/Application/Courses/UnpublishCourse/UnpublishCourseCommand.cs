using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Courses.UnpublishCourse;

public sealed record UnpublishCourseCommand(string UserId, int CourseId) : IRequest<Result<CourseReadinessResponseDto>>;