using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Application.Chat.SearchCommunities;

public class SearchCommunitiesQueryHandler(
    ICommunityRepository repository
) : IRequestHandler<SearchCommunitiesQuery, Result<List<SearchCommunitiesResponseDto>>>
{
    public async Task<Result<List<SearchCommunitiesResponseDto>>> Handle(SearchCommunitiesQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
            return Result.Success(new List<SearchCommunitiesResponseDto>());

        var communities = await repository.SearchCommunities(request.Query, request.Offset, request.Limit);

        if (communities == null || communities.Count == 0) 
            return Result.NotFound("Не найдено ни одного сообщества с таким именем");

        return Result.Success(communities);
    }
}