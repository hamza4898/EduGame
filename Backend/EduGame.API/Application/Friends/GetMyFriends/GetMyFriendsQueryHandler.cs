using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Friends.GetMyFriends;

public class GetMyFriendsQueryHandler(IFriendshipRepository repository) : IRequestHandler<GetMyFriendsQuery, Result<List<FriendResponseDto>>>
{
    public async Task<Result<List<FriendResponseDto>>> Handle(GetMyFriendsQuery request, CancellationToken cancellationToken)
    {
        var friends = await repository.GetUserFriendDetails(request.UserId);
        return Result.Success(friends);
    }
}