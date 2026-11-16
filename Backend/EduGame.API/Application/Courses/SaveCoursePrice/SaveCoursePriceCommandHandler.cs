using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Application.Courses.SaveCoursePrice;

public sealed class SaveCoursePriceCommandHandler(
    ICourseManagementService service,
    ICourseRepository courseRepository)
    : IRequestHandler<SaveCoursePriceCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(SaveCoursePriceCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId)) return Result.Unauthorized("Не удалось определить пользователя.");
        if (request.CourseId <= 0) return Result.Invalid(new ValidationError("CourseId", "Идентификатор курса невалиден."));
        if (request.Request is null) return Result.Invalid(new ValidationError("Request", "Данные запроса пусты."));

        var courseExists = await courseRepository.IsCourseExistsAsync(request.CourseId, cancellationToken);
        if (!courseExists) return Result.NotFound($"Курс с ID {request.CourseId} не найден.");

        var hasAccess = await courseRepository.CanEditCourseAsync(request.UserId, request.CourseId, cancellationToken);
        if (!hasAccess) return Result.Forbidden("У вас нет прав на изменение стоимости этого курса.");

        await service.SavePriceAsync(request.UserId, request.CourseId, request.Request, cancellationToken);
        
        return Result.Success(true); 
    }
}