using MediatR;
using Ardalis.Result;
using EduGame.Domain.Entities;
using EduGame.Domain.Enums;
using EduGame.Web.Hubs;
using EduGame.Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace EduGame.Application.Communities.SendCommunityInvite;

public class SendCommunityInviteCommandHandler(
    ICommunityRepository repository,
    IHubContext<NotificationHub> hubContext)
    : IRequestHandler<SendCommunityInviteCommand, Result>
{
    public async Task<Result> Handle(SendCommunityInviteCommand request, CancellationToken cancellationToken)
    {
        var senderUserName = await repository.GetSenderUserNameAsync(request.SenderId, cancellationToken);

        if (senderUserName is null) return Result.NotFound("Отправитель не найден.");

        var isManager = await repository.IsUserCommunityManagerAsync(request.CommunityId, request.SenderId, cancellationToken);

        if (!isManager) return Result.Forbidden("Приглашать участников могут только администраторы сообщества.");

        var areFriends = await repository.AreUsersFriendsAsync(request.SenderId, request.ReceiverId, cancellationToken);

        if (!areFriends) return Result.Forbidden("Приглашать можно только пользователей из списка друзей.");

        var community = await repository.GetCommunityByGuid(request.CommunityId);

        if (community is null) return Result.NotFound("Сообщество не найдено.");

        var isMember = await repository.IsUserInCommunity(request.CommunityId, request.ReceiverId);

        if (isMember) return Result.Conflict("Пользователь уже состоит в этом сообществе.");

        if (community.MembersLimit != CommunityMembersLimit.Unlimited
            && community.MembersCount >= (int)community.MembersLimit)
            return Result.Conflict("В сообществе больше нет свободных мест.");

        var receiverExists = await repository.IsUserExistsAsync(request.ReceiverId, cancellationToken);

        if (!receiverExists) return Result.NotFound("Получатель приглашения не найден.");

        var messageText = $"{senderUserName ?? request.SenderId} приглашает вас в сообщество «{community.CommunityName}».";

        var notification = new Notification
        {
            SenderId = request.SenderId,
            ReceiverId = request.ReceiverId,
            Message = messageText,
            Type = "CommunityInvite",
            TargetId = community.CommunityId,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        repository.AddNotification(notification);
        await repository.Save();

        await hubContext.Clients.User(request.ReceiverId).SendAsync(
            "ReceiveNotification",
            new 
            {
                id = notification.Id,
                message = notification.Message,
                type = notification.Type,
                targetId = notification.TargetId,
                createdAt = notification.CreatedAt
            }, 
            cancellationToken
        );

        return Result.Success();
    }
}