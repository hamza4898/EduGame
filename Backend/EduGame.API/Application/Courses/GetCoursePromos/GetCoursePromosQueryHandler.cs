using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Courses.GetCoursePromos;

public sealed class GetCoursePromosQueryHandler(
    ICourseManagementService service,
    ICourseRepository courseRepository) 
    : IRequestHandler<GetCoursePromosQuery, Result<List<CoursePromoResponseDto>>>
{
    public async Task<Result<List<CoursePromoResponseDto>>> Handle(GetCoursePromosQuery request, CancellationToken cancellationToken)
    {
        if (request.CourseId <= 0) return Result.Invalid(new ValidationError("CourseId", "Идентификатор курса должен быть больше нуля."));

        if (string.IsNullOrWhiteSpace(request.UserId)) return Result.Unauthorized("Не удалось определить пользователя.");

        var courseExists = await courseRepository.IsCourseExistsAsync(request.CourseId, cancellationToken);

        if (!courseExists) return Result.NotFound($"Курс с ID {request.CourseId} не найден.");

        var hasAccess = await courseRepository.CanEditCourseAsync(request.UserId, request.CourseId, cancellationToken);

        if (!hasAccess) return Result.Forbidden("У вас нет прав на просмотр списка промокодов этого курса.");
        
        var promos = await service.GetPromosAsync(request.UserId, request.CourseId, cancellationToken);

        return Result.Success(promos.ToList());
    }
}