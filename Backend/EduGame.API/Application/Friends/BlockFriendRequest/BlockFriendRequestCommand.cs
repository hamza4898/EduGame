using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Friends.BlockFriendRequest;

public record BlockFriendRequestCommand(string UserId, int RequestId) : IRequest<Result<Unit>>;