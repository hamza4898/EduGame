using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Contracts;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Application.Courses.UnpublishCourse;

public sealed class UnpublishCourseCommandHandler(
    ICourseManagementService service,
    ICourseRepository courseRepository)
    : IRequestHandler<UnpublishCourseCommand, Result<CourseReadinessResponseDto>>
{
    public async Task<Result<CourseReadinessResponseDto>> Handle(UnpublishCourseCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId)) return Result.Unauthorized("Не удалось определить пользователя.");
        if (request.CourseId <= 0) return Result.Invalid(new ValidationError("CourseId", "Идентификатор курса невалиден."));

        var courseExists = await courseRepository.IsCourseExistsAsync(request.CourseId, cancellationToken);
        if (!courseExists) return Result.NotFound($"Курс с ID {request.CourseId} не найден.");

        var hasAccess = await courseRepository.CanEditCourseAsync(request.UserId, request.CourseId, cancellationToken);
        if (!hasAccess) return Result.Forbidden("У вас нет прав на изменение статуса публикации этого курса.");

        var result = await service.UnpublishAsync(request.UserId, request.CourseId, cancellationToken);
        return Result.Success(result);
    }
}