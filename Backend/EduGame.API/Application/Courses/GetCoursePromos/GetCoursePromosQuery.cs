using Ardalis.Result;
using MediatR;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Courses.GetCoursePromos;

public sealed record GetCoursePromosQuery(string UserId, int CourseId) 
    : IRequest<Result<List<CoursePromoResponseDto>>>;