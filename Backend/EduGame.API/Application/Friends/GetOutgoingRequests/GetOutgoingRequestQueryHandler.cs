using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Friends.GetOutgoingRequests;

public class GetOutgoingRequestQueryHandler(IFriendshipRepository repository) : IRequestHandler<GetOutgoingRequestQuery, Result<List<FriendshipRequestResponseDto>>>
{
    public async Task<Result<List<FriendshipRequestResponseDto>>> Handle(GetOutgoingRequestQuery request, CancellationToken cancellationToken)
    {
        var userOutgoingRequests = await repository.GetOutgoingRequests(request.UserId);

        if (userOutgoingRequests == null) return Result.Success(new List<FriendshipRequestResponseDto>());

        return Result.Success(userOutgoingRequests);
    }
}