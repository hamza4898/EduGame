using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Courses.GetMyLikedTeacherCourses;

public class GetMyLikedTeacherCoursesQueryHandler(
    IUserRepository teacherRepository,
    ICourseRepository courseRepository,
    ICourseReadinessService readinessService)
    : IRequestHandler<GetMyLikedTeacherCoursesQuery, Result<List<MyTeacherCourseDto>>>
{
    public async Task<Result<List<MyTeacherCourseDto>>> Handle(GetMyLikedTeacherCoursesQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.TeacherId)) return Result.Unauthorized("Не удалось определить пользователя.");

        if (await teacherRepository.GetTeacherById(request.TeacherId) is null) return Result.Forbidden("Избранное доступно только преподавателю.");

        var likedIds = await courseRepository.GetLikedCourseIdsAsync(request.TeacherId, cancellationToken);

        if (likedIds.Count == 0) return Result.Success(new List<MyTeacherCourseDto>());

        var archivedIds = await courseRepository.GetArchivedCourseIdsAsync(request.TeacherId, cancellationToken);

        var courses = await courseRepository.GetCoursesByTeacherId(request.TeacherId, cancellationToken);
        
        var likedCourses = courses
            .Where(course => likedIds.Contains(course.Id) && !archivedIds.Contains(course.Id))
            .Select(course => readinessService.MapToTeacherDto(course, request.TeacherId)) // Чистый вызов через сервис
            .ToList();

        return Result.Success(likedCourses);
    }
}