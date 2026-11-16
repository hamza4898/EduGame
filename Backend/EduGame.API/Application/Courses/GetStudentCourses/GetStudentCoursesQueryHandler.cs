using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Contracts;
using AutoMapper;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Application.Courses.GetStudentCourses;

public class GetStudentCoursesQueryHandler(
    IStudentCourseRepository studentCourseRepository,
    ICourseRepository courseRepository,
    IMapper mapper
    ) : IRequestHandler<GetStudentCoursesQuery, Result<List<CatalogCoursesDto>>>
{
    public async Task<Result<List<CatalogCoursesDto>>> Handle(GetStudentCoursesQuery request, CancellationToken cancellationToken)
    {
        var enrolledRecords = await studentCourseRepository.GetRawStudentCoursesAsync(request.StudentId);

        if (enrolledRecords == null || enrolledRecords.Count == 0) return Result.NotFound("Вы еще не записаны ни на один курс.");

        var courseIds = enrolledRecords.Select(sc => sc.CourseId).ToList();

        var catalogDtos = await courseRepository.GetCoursesByIdsAsync(courseIds, mapper.ConfigurationProvider);

        var archives = await courseRepository.GetUserCourseArchivesAsync(
            request.StudentId,
            courseIds,
            cancellationToken);

        var likes = await courseRepository.GetUserCourseLikesAsync(
            request.StudentId,
            courseIds,
            cancellationToken);

        var enrolledByCourse = enrolledRecords.ToDictionary(record => record.CourseId);

        var finalResult = new List<CatalogCoursesDto>(catalogDtos.Count);

        foreach (var dto in catalogDtos)
        {
            var record = enrolledByCourse[dto.Id];
            dto.Owned = true;
            dto.Progress = record.ProgressPercent;
            dto.IsArchived = archives.Contains(dto.Id);
            dto.IsLiked = likes.Contains(dto.Id);
            finalResult.Add(dto);
        }

        return Result.Success(finalResult);
    }
}