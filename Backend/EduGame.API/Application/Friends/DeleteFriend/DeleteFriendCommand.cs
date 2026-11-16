using Ardalis.Result;
using MediatR;

namespace EduGame.Application.Friends.DeleteFriend;

public record DeleteFriendCommand(string UserId, int RequestId) : IRequest<Result<Unit>>;