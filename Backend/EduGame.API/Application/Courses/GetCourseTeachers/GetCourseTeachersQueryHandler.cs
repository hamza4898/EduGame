using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Contracts;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Application.Courses.GetCourseTeachers;

public sealed class GetCourseTeachersQueryHandler(
    ICourseManagementService service,
    ICourseRepository courseRepository) 
    : IRequestHandler<GetCourseTeachersQuery, Result<List<CourseTeacherResponseDto>>>
{
    public async Task<Result<List<CourseTeacherResponseDto>>> Handle(GetCourseTeachersQuery request, CancellationToken cancellationToken)
    {
        if (request.CourseId <= 0) return Result.Invalid(new ValidationError("CourseId", "Идентификатор курса должен быть больше нуля."));

        if (string.IsNullOrWhiteSpace(request.UserId)) return Result.Unauthorized("Не удалось определить пользователя.");

        var courseExists = await courseRepository.IsCourseExistsAsync(request.CourseId, cancellationToken);
        if (!courseExists) return Result.NotFound($"Курс с ID {request.CourseId} не найден.");

        var hasAccess = await courseRepository.CanEditCourseAsync(request.UserId, request.CourseId, cancellationToken);
        if (!hasAccess) return Result.Forbidden("У вас нет прав на просмотр списка преподавателей этого курса.");

        var teachers = await service.GetTeachersAsync(request.UserId, request.CourseId, cancellationToken);

        return Result.Success(teachers.ToList());
    }
}