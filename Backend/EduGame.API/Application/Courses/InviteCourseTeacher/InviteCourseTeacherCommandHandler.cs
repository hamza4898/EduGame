using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Application.Courses.InviteCourseTeacher;

public class InviteCourseTeacherCommandHandler(
    ICourseManagementService service,
    ICourseRepository courseRepository)
    : IRequestHandler<InviteCourseTeacherCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(InviteCourseTeacherCommand request, CancellationToken cancellationToken)
    {
        if (request.CourseId <= 0) 
            return Result.Invalid(new ValidationError("CourseId", "Идентификатор курса невалиден."));
            
        if (request.Request is null || string.IsNullOrWhiteSpace(request.Request.UserName))
            return Result.Invalid(new ValidationError("UserName", "Никнейм преподавателя должен быть указан."));

        var courseExists = await courseRepository.IsCourseExistsAsync(request.CourseId, cancellationToken);

        if (!courseExists) return Result.NotFound($"Курс с ID {request.CourseId} не найден.");

        var hasAccess = await courseRepository.CanEditCourseAsync(request.UserId, request.CourseId, cancellationToken);

        if (!hasAccess) return Result.Forbidden("У вас нет прав на добавление преподавателей к этому курсу.");

        await service.InviteTeacherAsync(request.UserId, request.CourseId, request.Request, cancellationToken);
        
        return Result.Success(true);
    }
}