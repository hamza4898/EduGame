using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Courses.PublishCourseNews;

public sealed class PublishCourseNewsCommandHandler(
    ICourseManagementService service,
    ICourseRepository courseRepository)
    : IRequestHandler<PublishCourseNewsCommand, Result<CourseNewsResponseDto>>
{
    public async Task<Result<CourseNewsResponseDto>> Handle(PublishCourseNewsCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId)) return Result.Unauthorized("Не удалось определить пользователя.");
        if (request.CourseId <= 0) return Result.Invalid(new ValidationError("CourseId", "Идентификатор курса невалиден."));

        var courseExists = await courseRepository.IsCourseExistsAsync(request.CourseId, cancellationToken);
        if (!courseExists) return Result.NotFound($"Курс с ID {request.CourseId} не найден.");

        var hasAccess = await courseRepository.CanEditCourseAsync(request.UserId, request.CourseId, cancellationToken);
        if (!hasAccess) return Result.Forbidden("У вас нет прав на создание публикаций в данном курсе.");

        var result = await service.PublishNewsAsync(request.UserId, request.CourseId, request.Request, cancellationToken);
        return Result.Success(result);
    }
}