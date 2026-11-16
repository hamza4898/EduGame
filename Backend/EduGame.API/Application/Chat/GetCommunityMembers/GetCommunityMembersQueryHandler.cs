using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using MediatR;

namespace EduGame.Application.Chat.GetCommunityMembers;

public class GetCommunityMembersQueryHandler(
    ICommunityRepository repository
) : IRequestHandler<GetCommunityMembersQuery, Result<List<GetCommunityMembersResponseDto>>>
{
    public async Task<Result<List<GetCommunityMembersResponseDto>>> Handle(GetCommunityMembersQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId)) return Result.Unauthorized("Не удалось определить пользователя.");

        var isUserInCommunity = await repository.IsUserInCommunity(request.CommunityId, request.UserId);

        if (!isUserInCommunity) return Result.Forbidden("Данного участника нет в данной группе");

        var communityMembers = await repository.GetCommunityMembers(
            request.CommunityId,
            request.UserId,
            Math.Max(0, request.Offset),
            Math.Clamp(request.Limit, 1, 100)
        );

        return Result.Success(communityMembers);
    } 
}