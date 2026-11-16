using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using EduGame.Domain.Entities;

namespace EduGame.Application.Friends.GetFriendsAvailableForCommunity;

public class GetFriendsAvailableForCommunityQueryHandler(
    ICommunityRepository communityRepository,
    IFriendshipRepository friendshipRepository)
    : IRequestHandler<GetFriendsAvailableForCommunityQuery, Result<List<BaseUser>>>
{
    public async Task<Result<List<BaseUser>>> Handle(GetFriendsAvailableForCommunityQuery request, CancellationToken cancellationToken)
    {
        var existingMemberIds = await communityRepository.GetCommunityMemberIds(request.CommunityId);

        var allFriends = await friendshipRepository.GetUserFriends(request.UserId);

        if (allFriends == null || !allFriends.Any()) return Result.Success(new List<BaseUser>());

        var availableFriends = allFriends
            .Where(friend => !existingMemberIds.Contains(friend.Id))
            .ToList();

        return Result.Success(availableFriends);
    }
}
