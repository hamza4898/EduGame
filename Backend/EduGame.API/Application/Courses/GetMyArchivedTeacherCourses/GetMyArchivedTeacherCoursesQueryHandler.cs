using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Courses.GetMyArchivedTeacherCourses;

internal sealed class GetMyArchivedTeacherCoursesQueryHandler(
    IUserRepository teacherRepository,
    ICourseRepository courseRepository,
    ICourseReadinessService readinessService)
    : IRequestHandler<GetMyArchivedTeacherCoursesQuery, Result<List<MyTeacherCourseDto>>>
{
    public async Task<Result<List<MyTeacherCourseDto>>> Handle(GetMyArchivedTeacherCoursesQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.TeacherId)) return Result.Unauthorized("Не удалось определить пользователя.");

        if (await teacherRepository.GetTeacherById(request.TeacherId) is null) return Result.Forbidden("Архив доступен только преподавателю.");

        var archivedIds = await courseRepository.GetArchivedCourseIdsAsync(request.TeacherId, cancellationToken);

        if (archivedIds.Count == 0) return Result.Success(new List<MyTeacherCourseDto>());

        var courses = await courseRepository.GetCoursesByTeacherId(request.TeacherId, cancellationToken);
        
        var archivedCourses = courses
            .Where(course => archivedIds.Contains(course.Id))
            .Select(course => readinessService.MapToTeacherDto(course, request.TeacherId))
            .ToList();

        return Result.Success(archivedCourses);
    }
}