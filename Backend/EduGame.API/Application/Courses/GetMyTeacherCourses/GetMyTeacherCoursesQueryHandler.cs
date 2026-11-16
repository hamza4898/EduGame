using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Courses.GetMyTeacherCourses;

public class GetMyTeacherCoursesQueryHandler(
    IUserRepository teacherRepository,
    ICourseRepository courseRepository,
    ICourseReadinessService readinessService)
    : IRequestHandler<GetMyTeacherCoursesQuery, Result<List<MyTeacherCourseDto>>>
{
    public async Task<Result<List<MyTeacherCourseDto>>> Handle(GetMyTeacherCoursesQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.TeacherId)) return Result.Unauthorized("Не удалось определить пользователя.");

        if (await teacherRepository.GetTeacherById(request.TeacherId) is null) return Result.Forbidden("Список курсов доступен только преподавателю.");

        var archivedCourseIds = await courseRepository.GetArchivedCourseIdsAsync(request.TeacherId, cancellationToken);

        var courses = await courseRepository.GetCoursesByTeacherId(request.TeacherId, cancellationToken);
        
        var activeCourses = courses
            .Where(course => !archivedCourseIds.Contains(course.Id))
            .Select(course => readinessService.MapToTeacherDto(course, request.TeacherId))
            .ToList();

        return Result.Success(activeCourses);
    }
}