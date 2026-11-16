using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Application.Courses.GetMyTeacherCourseStudentCounts;

public class GetMyTeacherCourseStudentCountsQueryHandler(IUserRepository teacherRepository, ICourseRepository courseRepository)
    : IRequestHandler<GetMyTeacherCourseStudentCountsQuery, Result<List<TeacherCourseStudentCountDto>>>
{
    public async Task<Result<List<TeacherCourseStudentCountDto>>> Handle(GetMyTeacherCourseStudentCountsQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.TeacherId)) return Result.Unauthorized("Не удалось определить пользователя.");

        var teacher = await teacherRepository.GetTeacherById(request.TeacherId);

        if (teacher is null) return Result.Forbidden("Статистика доступна только преподавателю.");

        var counts = await courseRepository.GetTeacherCoursesStudentCountsAsync(request.TeacherId, cancellationToken);

        return Result.Success(counts);
    }
}