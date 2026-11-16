using EduGame.Application.Common.Interfaces;
using EduGame.Application.Communities.GetCommunityRatings;

namespace EduGame.Infrastructure.Services;

public class CommunityRatingService(
    IRatingRepository ratingRepository,
    IRatingCalculatorService ratingCalculatorService
    ) : ICommunityRatingService
{
    public async Task<List<CommunityRatingResponseDto>> GetTopRatingsAsync(int count, CancellationToken cancellationToken)
    {
        var rawCommunities = await ratingRepository.GetRawCommunityRatingsAsync(null!, cancellationToken);

        return rawCommunities.Select(com => 
        {
            var avgProgress = com.MaxPossibleXp > 0 ? ((double)com.TotalXp / com.MaxPossibleXp) * 100.0 : 0.0;
            if (avgProgress > 100.0) avgProgress = 100.0;

            return new CommunityRatingResponseDto
            {
                Id = com.Id,
                Name = com.Name,
                PowerIndex = ratingCalculatorService.CalculateCommunityPowerIndex(avgProgress, com.TotalXp)
            };
        })
        .OrderByDescending(com => com.PowerIndex)
        .Take(count)
        .ToList();
    }
}