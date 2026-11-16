using Ardalis.Result;
using EduGame.Application.Common.Contracts;
using MediatR;

namespace EduGame.Application.Courses.GetCourseReadiness;

public sealed record GetCourseReadinessQuery(string UserId, int CourseId) 
    : IRequest<Result<CourseReadinessResponseDto>>;