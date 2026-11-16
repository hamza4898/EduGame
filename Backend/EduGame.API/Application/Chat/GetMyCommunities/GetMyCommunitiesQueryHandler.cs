using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Application.Chat.GetMyCommunities;

public class GetMyCommunitiesQueryHandler(ICommunityRepository repository) : IRequestHandler<GetMyCommunitiesQuery, Result<List<MyCommunitiesResponseDto>>>
{
    public async Task<Result<List<MyCommunitiesResponseDto>>> Handle(GetMyCommunitiesQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId)) return Result.Unauthorized("Не удалось определить пользователя.");

        var userCommunities = await repository.GetUserCommunities(request.UserId);

        return Result.Success(userCommunities);
    }
}