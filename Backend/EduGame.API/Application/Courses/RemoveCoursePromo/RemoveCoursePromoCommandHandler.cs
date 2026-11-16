using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Application.Courses.RemoveCoursePromo;

public class RemoveCoursePromoCommandHandler(
    ICourseManagementService service,
    ICourseRepository courseRepository)
    : IRequestHandler<RemoveCoursePromoCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(RemoveCoursePromoCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId)) 
            return Result.Unauthorized("Не удалось определить пользователя.");
            
        if (request.CourseId <= 0) 
            return Result.Invalid(new ValidationError("CourseId", "Идентификатор курса невалиден."));
            
        if (request.PromoId <= 0) 
            return Result.Invalid(new ValidationError("PromoId", "Идентификатор промокода невалиден."));

        var courseExists = await courseRepository.IsCourseExistsAsync(request.CourseId, cancellationToken);

        if (!courseExists) return Result.NotFound($"Курс с ID {request.CourseId} не найден.");

        var hasAccess = await courseRepository.CanEditCourseAsync(request.UserId, request.CourseId, cancellationToken);

        if (!hasAccess) return Result.Forbidden("У вас нет прав на управление промокодами этого курса.");

        await service.RemovePromoAsync(request.UserId, request.CourseId, request.PromoId, cancellationToken);
        
        return Result.Success(true);
    }
}