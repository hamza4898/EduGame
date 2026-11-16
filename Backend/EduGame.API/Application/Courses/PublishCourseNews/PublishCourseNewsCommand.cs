using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Courses.PublishCourseNews;

public sealed record PublishCourseNewsCommand(string UserId, int CourseId, PublishCourseNewsRequestDto Request) : IRequest<Result<CourseNewsResponseDto>>;