using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Friends.AcceptFriendRequest;

public record AcceptFriendRequestCommand(string UserId, int RequestId) : IRequest<Result<Unit>>;