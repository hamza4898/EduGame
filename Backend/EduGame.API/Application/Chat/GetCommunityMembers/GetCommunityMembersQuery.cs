using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Chat.GetCommunityMembers;

public record GetCommunityMembersQuery(Guid CommunityId, string UserId, int Offset, int Limit) : IRequest<Result<List<GetCommunityMembersResponseDto>>>;