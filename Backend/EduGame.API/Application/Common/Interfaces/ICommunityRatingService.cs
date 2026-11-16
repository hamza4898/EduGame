using EduGame.Application.Communities.GetCommunityRatings;

namespace EduGame.Application.Common.Interfaces;

public interface ICommunityRatingService
{
    Task<List<CommunityRatingResponseDto>> GetTopRatingsAsync(int count, CancellationToken cancellationToken);
}