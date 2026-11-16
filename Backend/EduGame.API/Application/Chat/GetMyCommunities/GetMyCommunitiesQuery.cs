using Ardalis.Result;
using MediatR;

namespace EduGame.Application.Chat.GetMyCommunities;

public record GetMyCommunitiesQuery(string UserId) : IRequest<Result<List<MyCommunitiesResponseDto>>>;