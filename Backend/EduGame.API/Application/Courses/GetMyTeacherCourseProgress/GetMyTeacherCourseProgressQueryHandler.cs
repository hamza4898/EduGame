using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Application.Courses.GetMyTeacherCourseProgress;

public class GetMyTeacherCourseProgressQueryHandler(
    IUserRepository teacherRepository,
    ICourseRepository courseRepository)
    : IRequestHandler<GetMyTeacherCourseProgressQuery, Result<List<TeacherCourseProgressDto>>>
{
    public async Task<Result<List<TeacherCourseProgressDto>>> Handle(GetMyTeacherCourseProgressQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.TeacherId)) return Result.Unauthorized("Не удалось определить пользователя.");

        var teacher = await teacherRepository.GetTeacherById(request.TeacherId);

        if (teacher is null) return Result.Forbidden("Статистика доступна только преподавателю.");

        var progress = await courseRepository.GetTeacherCoursesProgressAsync(request.TeacherId, cancellationToken);

        return Result.Success(progress);
    }
}