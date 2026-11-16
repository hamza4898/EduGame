using MediatR;
using Ardalis.Result;
using EduGame.Domain.Entities;
using EduGame.Domain.Enums;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Application.Chat.JoinCommunity;

public class JoinCommunityCommandHandler(
    ICommunityRepository communityRepository
    ) : IRequestHandler<JoinCommunityCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(JoinCommunityCommand request, CancellationToken cancellationToken)
    {
        var community = Guid.TryParse(request.CommunityName, out var communityGuid)
        ? await communityRepository.GetCommunityByGuid(communityGuid)
        : await communityRepository.GetCommunityByName(request.CommunityName);

        if (community == null) return Result.NotFound("Такого сообщества не существует");

        var isMemberExistsInCommunity = await communityRepository.IsUserInCommunity(community.CommunityId, request.UserId);

        if (isMemberExistsInCommunity) return Result.Conflict("Данный участник уже состоит в данной группе");

        if (community.IsFull) return Result.Conflict("Данная группа переполнена");

        var communityMember = new CommunityMember
        {
            CommunityId = community.CommunityId,
            UserId = request.UserId,
            UserName = request.UserName,
            AvatarUrl = "images/hamzaBro3.png",
            Role = ClanRole.Member
        };

        community.MembersCount++;

        communityRepository.AddMember(communityMember);
        await communityRepository.Save();

        return Result.Success();
    }
}