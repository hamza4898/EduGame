using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Communities.GetCommunityRatings;

public record GetCommunityRatingsQuery() : IRequest<Result<List<CommunityRatingResponseDto>>>;