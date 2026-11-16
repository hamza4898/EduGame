using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Communities.AcceptCommunityInvite;

public record AcceptCommunityInviteCommand(string UserId, Guid CommunityId, int NotificationId) : IRequest<Result>;