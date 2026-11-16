using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Application.Courses.GetMyTeacherCourseCompletion;

public class GetMyTeacherCourseCompletionCountsQueryHandler(
    IUserRepository teacherRepository,
    ICourseRepository courseRepository)
    : IRequestHandler<GetMyTeacherCourseCompletionCountsQuery, Result<List<TeacherCourseCompletionCountDto>>>
{
    public async Task<Result<List<TeacherCourseCompletionCountDto>>> Handle(GetMyTeacherCourseCompletionCountsQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.TeacherId)) return Result.Unauthorized("Не удалось определить пользователя.");

        var teacher = await teacherRepository.GetTeacherById(request.TeacherId);
        
        if (teacher is null) return Result.Forbidden("Статистика доступна только преподавателю.");

        var completedCounts = await courseRepository.GetTeacherCoursesCompletionCountsAsync(request.TeacherId, cancellationToken);

        return Result.Success(completedCounts);
    }
}