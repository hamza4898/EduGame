using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using EduGame.Application.Common.Contracts;
using AutoMapper;

namespace EduGame.Application.Courses.GetCatalogCourses;

public class GetCatalogCoursesQueryHandler(
    ICourseRepository courseRepository,
    IStudentCourseRepository studentCourseRepository,
    IMapper mapper) : IRequestHandler<GetCatalogCoursesQuery, Result<List<CatalogCoursesDto>>>
{
    public async Task<Result<List<CatalogCoursesDto>>> Handle(GetCatalogCoursesQuery request, CancellationToken cancellationToken)
    {
        var query = courseRepository.BaseQuery();

        if (!string.IsNullOrWhiteSpace(request.Query))
            query = courseRepository.FilterBySearch(query, request.Query);

        if (!string.IsNullOrWhiteSpace(request.PriceFilter) && request.PriceFilter.ToLower() != "all")
            query = courseRepository.FilterByPrice(query, request.PriceFilter);
        
        if (request.PathId.HasValue)
            query = courseRepository.FilterByPath(query, request.PathId.Value);
        
        if (request.SubPathId.HasValue)
            query = courseRepository.FilterBySubPath(query, request.SubPathId.Value);

        var catalog = await courseRepository.ExecuteToCatalogListAsync(query, mapper.ConfigurationProvider);
        var courseIds = catalog.Select(course => course.Id).ToList();

        if (courseIds.Count > 0)
        {
            var studentCounts = await courseRepository.GetCoursesByIdsAsync(courseIds, mapper.ConfigurationProvider);

            foreach (var course in catalog)
            {
                var matched = studentCounts.FirstOrDefault(c => c.Id == course.Id);
                course.StudentsCount = matched?.StudentsCount ?? 0;
            }
        }

        if (!string.IsNullOrWhiteSpace(request.UserId) && courseIds.Count > 0)
        {
            var enrollments = await studentCourseRepository.GetRawStudentCoursesAsync(request.UserId);
            
            var userLikes = await courseRepository.GetUserCourseLikesAsync(request.UserId, courseIds, cancellationToken);
            
            var userArchives = await courseRepository.GetUserCourseArchivesAsync(request.UserId, courseIds, cancellationToken);

            foreach (var course in catalog)
            {
                var enrollment = enrollments.FirstOrDefault(item => item.CourseId == course.Id);
                course.Owned = enrollment is not null;
                course.Progress = enrollment?.ProgressPercent ?? 0;
                
                course.IsLiked = userLikes.Contains(course.Id);
                course.IsArchived = userArchives.Contains(course.Id);
            }
        }

        return Result.Success(catalog);
    }
}