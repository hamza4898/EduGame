using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Communities.SendCommunityInvite;

public record SendCommunityInviteCommand(
    string SenderId,
    string ReceiverId,
    Guid CommunityId,
    string CommunityName
) : IRequest<Result>;