using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Friends.DeclineFriendRequest;

public record DeclineFriendRequestCommand(string UserId, int RequestId) : IRequest<Result<Unit>>;