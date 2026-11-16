using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Application.Courses.GetCourseRatings;

public class GetCourseRatingsQueryHandler(
    IRatingRepository ratingRepository,
    IRatingCalculatorService ratingCalculatorService
    ) : IRequestHandler<GetCourseRatingsQuery, Result<List<CourseRatingResponseDto>>>
{
    public async Task<Result<List<CourseRatingResponseDto>>> Handle(GetCourseRatingsQuery request, CancellationToken cancellationToken)
    {
        var rawCourses = await ratingRepository.GetRawCourseRatingsAsync(null!, cancellationToken);

        var result = rawCourses.Select(c => new CourseRatingResponseDto
        {
            Id = c.Id,
            Title = c.Title,
            PopIndex = ratingCalculatorService.CalculateCoursePopularityIndex(c.Rating, c.StudentsCount, c.AvgProgress)
        })
        .OrderByDescending(c => c.PopIndex)
        .Take(10)
        .ToList();

        return Result.Success(result);
    }
}