using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Courses.GetCourseNews;

public sealed class GetCourseNewsQueryHandler(
    ICourseManagementService service,
    ICourseRepository courseRepository)
    : IRequestHandler<GetCourseNewsQuery, Result<List<CourseNewsResponseDto>>>
{
    public async Task<Result<List<CourseNewsResponseDto>>> Handle(GetCourseNewsQuery request, CancellationToken cancellationToken)
    {
        if (request.CourseId <= 0) return Result.Invalid(new ValidationError("CourseId", "Идентификатор курса должен быть больше нуля."));

        if (string.IsNullOrWhiteSpace(request.UserId)) return Result.Unauthorized("Не удалось определить авторизованного пользователя.");

        var courseExists = await courseRepository.IsCourseExistsAsync(request.CourseId, cancellationToken);

        if (!courseExists) return Result.NotFound($"Курс с ID {request.CourseId} не найден.");

        var isPublished = await courseRepository.IsCoursePublishedAsync(request.CourseId, cancellationToken);
        
        if (!isPublished)
        {
            var hasAccess = await courseRepository.CanEditCourseAsync(request.UserId, request.CourseId, cancellationToken);
            if (!hasAccess) return Result.Forbidden("У вас нет прав на просмотр новостей неопубликованного курса.");
        }

        var newsList = await service.GetNewsAsync(request.UserId, request.CourseId, cancellationToken);

        return Result.Success(newsList.ToList());
    }
}