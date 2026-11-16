using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Chat.JoinCommunity;

public record JoinCommunityCommand(string CommunityName, string UserId, string UserName) : IRequest<Result<Unit>>;