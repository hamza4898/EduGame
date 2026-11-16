using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Chat.SearchCommunities;

public record SearchCommunitiesQuery(string Query, int Offset, int Limit) : IRequest<Result<List<SearchCommunitiesResponseDto>>>;