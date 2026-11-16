using Ardalis.Result;
using EduGame.Application.Common.Contracts;
using MediatR;

namespace EduGame.Application.Courses.GetCourseNews;

public sealed record GetCourseNewsQuery(string UserId, int CourseId) 
    : IRequest<Result<List<CourseNewsResponseDto>>>;