using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using EduGame.Web.Hubs;
using EduGame.Domain.Entities;

namespace EduGame.Application.Developer.BroadcastDeveloperNotification;

public sealed class BroadcastDeveloperNotificationCommandHandler(
    IIdentityRepository identityRepository,
    INotificationRepository notificationRepository,
    IHubContext<NotificationHub> notificationHub)
    : IRequestHandler<BroadcastDeveloperNotificationCommand, Result<int>>
{
    public async Task<Result<int>> Handle(BroadcastDeveloperNotificationCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SenderId))
            return Result.Unauthorized("Не удалось определить администратора.");

        var message = request.Message.Trim();
        if (message.Length is 0 or > 2000)
            return Result.Invalid(new ValidationError("Message", "Уведомление должно содержать от 1 до 2000 символов."));

        var userIds = await identityRepository.GetAllUserIds(cancellationToken);
        var now = DateTime.UtcNow;
        var notifications = userIds.Select(receiverId => new Notification
        {
            ReceiverId = receiverId,
            SenderId = request.SenderId,
            Message = message,
            Type = "DeveloperBroadcast",
            CreatedAt = now
        }).ToArray();

        await notificationRepository.AddBroadcastAsync(notifications, cancellationToken);
        await notificationHub.Clients.All.SendAsync(
            "ReceiveNotification",
            new { message },
            cancellationToken);

        return Result.Success(notifications.Length);
    }
}