using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Courses.GetMyTeacherCourseStudents;

public class GetMyTeacherCourseStudentsQueryHandler(
    IUserRepository teacherRepository,
    ICourseRepository courseRepository)
    : IRequestHandler<GetMyTeacherCourseStudentsQuery, Result<List<TeacherCourseStudentDto>>>
{
    public async Task<Result<List<TeacherCourseStudentDto>>> Handle(GetMyTeacherCourseStudentsQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.TeacherId)) return Result.Unauthorized("Не удалось определить пользователя.");

        var teacher = await teacherRepository.GetTeacherById(request.TeacherId);

        if (teacher is null) return Result.Forbidden("Список учащихся доступен только преподавателю.");

        var students = await courseRepository.GetTeacherCourseStudentsAsync(request.TeacherId, cancellationToken);

        return Result.Success(students);
    }
}