using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Courses.GetCourseReviews;

public sealed class GetCourseReviewsQueryHandler(
    ICourseManagementService service,
    ICourseRepository courseRepository)
    : IRequestHandler<GetCourseReviewsQuery, Result<List<CourseReviewResponseDto>>>
{
    public async Task<Result<List<CourseReviewResponseDto>>> Handle(GetCourseReviewsQuery request, CancellationToken cancellationToken)
    {
        if (request.CourseId <= 0) return Result.Invalid(new ValidationError("CourseId", "Идентификатор курса должен быть больше нуля."));

        var courseExists = await courseRepository.IsCourseExistsAsync(request.CourseId, cancellationToken);

        if (!courseExists) return Result.NotFound($"Курс с ID {request.CourseId} не найден в системе.");

        var isPublished = await courseRepository.IsCoursePublishedAsync(request.CourseId, cancellationToken);

        if (!isPublished) return Result.Success(new List<CourseReviewResponseDto>());

        var reviews = await service.GetReviewsAsync(request.CourseId, cancellationToken);

        return Result.Success(reviews.ToList());
    }
}