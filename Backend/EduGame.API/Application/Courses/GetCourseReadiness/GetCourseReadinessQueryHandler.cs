using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Courses.GetCourseReadiness;

public sealed class GetCourseReadinessQueryHandler(
    ICourseManagementService service,
    ICourseRepository courseRepository)
    : IRequestHandler<GetCourseReadinessQuery, Result<CourseReadinessResponseDto>>
{
    public async Task<Result<CourseReadinessResponseDto>> Handle(GetCourseReadinessQuery request, CancellationToken cancellationToken)
    {
        if (request.CourseId <= 0) return Result.Invalid(new ValidationError("CourseId", "Идентификатор курса должен быть больше нуля."));

        if (string.IsNullOrWhiteSpace(request.UserId)) return Result.Unauthorized("Не удалось определить авторизованного пользователя.");

        var courseExists = await courseRepository.IsCourseExistsAsync(request.CourseId, cancellationToken);

        if (!courseExists) return Result.NotFound($"Курс с ID {request.CourseId} не найден в системе.");

        var hasAccess = await courseRepository.CanEditCourseAsync(request.UserId, request.CourseId, cancellationToken);

        if (!hasAccess) return Result.Forbidden("У вас нет прав на просмотр готовности этого курса.");

        var readinessResponse = await service.GetReadinessAsync(request.UserId, request.CourseId, cancellationToken);

        return Result.Success(readinessResponse);
    }
}