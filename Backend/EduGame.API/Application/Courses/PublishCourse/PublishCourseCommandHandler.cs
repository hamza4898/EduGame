using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Courses.PublishCourse;

public sealed class PublishCourseCommandHandler(
    ICourseManagementService service,
    ICourseRepository courseRepository)
    : IRequestHandler<PublishCourseCommand, Result<CourseReadinessResponseDto>>
{
    public async Task<Result<CourseReadinessResponseDto>> Handle(PublishCourseCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId)) return Result.Unauthorized("Не удалось определить пользователя.");
        if (request.CourseId <= 0) return Result.Invalid(new ValidationError("CourseId", "Идентификатор курса невалиден."));

        var courseExists = await courseRepository.IsCourseExistsAsync(request.CourseId, cancellationToken);
        if (!courseExists) return Result.NotFound($"Курс с ID {request.CourseId} не найден.");

        var hasAccess = await courseRepository.CanEditCourseAsync(request.UserId, request.CourseId, cancellationToken);
        if (!hasAccess) return Result.Forbidden("У вас нет прав на публикацию этого курса.");

        var result = await service.PublishAsync(request.UserId, request.CourseId, cancellationToken);
        return Result.Success(result);
    }
}