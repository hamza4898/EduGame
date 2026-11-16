using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Contracts;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Application.Friends.GetIncomingRequests;

public class GetIncomingRequestsQueryHandler(IFriendshipRepository repository) : IRequestHandler<GetIncomingRequestsQuery, Result<List<FriendshipRequestResponseDto>>>
{
    public async Task<Result<List<FriendshipRequestResponseDto>>> Handle(GetIncomingRequestsQuery request, CancellationToken cancellationToken)
    {
        var userIncomingRequests = await repository.GetIncomingRequests(request.UserId);

        if (userIncomingRequests == null) return Result.Success(new List<FriendshipRequestResponseDto>());

        return Result.Success(userIncomingRequests);
    }
}