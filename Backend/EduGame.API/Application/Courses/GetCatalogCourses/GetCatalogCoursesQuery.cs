using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Courses.GetCatalogCourses;

public record GetCatalogCoursesQuery(
    string? Query,
    string? PriceFilter,
    int? PathId,
    int? SubPathId,
    string? UserId = null
) : IRequest<Result<List<CatalogCoursesDto>>>;