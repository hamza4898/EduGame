using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Courses.GetTeacherRatings;

public class GetTeacherRatingsQueryHandler(
    ITeacherRatingService ratingService)
    : IRequestHandler<GetTeacherRatingsQuery, Result<List<TeacherRatingDto>>>
{
    public async Task<Result<List<TeacherRatingDto>>> Handle(GetTeacherRatingsQuery request, CancellationToken cancellationToken)
    {
        var ratings = await ratingService.GetTopTeacherRatingsAsync(10, cancellationToken);

        return Result.Success(ratings);
    }
}