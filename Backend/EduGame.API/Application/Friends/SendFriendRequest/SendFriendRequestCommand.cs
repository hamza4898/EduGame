using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Friends.SendFriendRequest;

public record SendFriendRequestCommand(string ReceiverId, string SenderId) : IRequest<Result<Unit>>;