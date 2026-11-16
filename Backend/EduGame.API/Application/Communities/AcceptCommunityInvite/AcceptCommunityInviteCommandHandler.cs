using MediatR;
using Ardalis.Result;
using EduGame.Domain.Entities;
using EduGame.Domain.Enums;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Application.Communities.AcceptCommunityInvite;

public class AcceptCommunityInviteCommandHandler(ICommunityRepository repository)
    : IRequestHandler<AcceptCommunityInviteCommand, Result>
{
    public async Task<Result> Handle(AcceptCommunityInviteCommand request, CancellationToken cancellationToken)
    {
        var notification = await repository.GetCommunityInviteNotificationAsync(
            request.NotificationId, 
            request.UserId, 
            request.CommunityId,
            cancellationToken);

        if (notification is null) return Result.NotFound("Приглашение не найдено или уже недоступно.");

        var community = await repository.GetCommunityByGuid(request.CommunityId);

        if (community is null) return Result.NotFound("Сообщество больше не существует.");

        var isMember = await repository.IsMemberExistsAsync(request.CommunityId, request.UserId, cancellationToken);

        if (!isMember)
        {
            if (community.MembersLimit != CommunityMembersLimit.Unlimited
                && community.MembersCount >= (int)community.MembersLimit)
                return Result.Conflict("В сообществе больше нет свободных мест.");

            var userResult = await repository.GetUserShortInfoAsync(request.UserId, cancellationToken);

            if (userResult is null) return Result.NotFound("Пользователь не найден.");

            repository.AddMember(new CommunityMember
            {
                CommunityId = community.CommunityId,
                UserId = request.UserId,
                UserName = userResult.Value.UserName ?? request.UserId,
                AvatarUrl = userResult.Value.AvatarUrl ?? string.Empty,
                Role = ClanRole.Member,
                JoinedAt = DateTime.UtcNow
            });
            
            community.MembersCount++;
        }

        notification.IsRead = true;
        
        await repository.Save(); 

        return Result.Success();
    }
}