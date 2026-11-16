using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Application.Courses.RemoveCourseTeacher;

public class RemoveCourseTeacherCommandHandler(
    ICourseManagementService service,
    ICourseRepository courseRepository)
    : IRequestHandler<RemoveCourseTeacherCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(RemoveCourseTeacherCommand request, CancellationToken cancellationToken)
    {            
        if (request.CourseId <= 0) 
            return Result.Invalid(new ValidationError("CourseId", "Идентификатор курса невалиден."));
            
        if (string.IsNullOrWhiteSpace(request.UserName))
            return Result.Invalid(new ValidationError("UserName", "Никнейм преподавателя должен быть указан."));

        var courseExists = await courseRepository.IsCourseExistsAsync(request.CourseId, cancellationToken);

        if (!courseExists) return Result.NotFound($"Курс с ID {request.CourseId} не найден.");

        var hasAccess = await courseRepository.CanEditCourseAsync(request.UserId, request.CourseId, cancellationToken);

        if (!hasAccess)  return Result.Forbidden("У вас нет прав на управление составом преподавателей этого курса.");

        await service.RemoveTeacherAsync(request.UserId, request.CourseId, request.UserName, cancellationToken);
        
        return Result.Success(true);
    }
}