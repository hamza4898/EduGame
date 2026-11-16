using MediatR;
using EduGame.Application.Common.Interfaces;
using Ardalis.Result;

namespace EduGame.Application.Communities.GetCommunityRatings;

public class GetCommunityRatingsQueryHandler(
    ICommunityRatingService ratingService
    ) : IRequestHandler<GetCommunityRatingsQuery, Result<List<CommunityRatingResponseDto>>>
{
    public async Task<Result<List<CommunityRatingResponseDto>>> Handle(GetCommunityRatingsQuery request, CancellationToken cancellationToken)
    {
        var result = await ratingService.GetTopRatingsAsync(10, cancellationToken);

        return Result.Success(result);
    }
}