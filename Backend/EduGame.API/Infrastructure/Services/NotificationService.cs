using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using EduGame.Application.Common.Interfaces;
using EduGame.Infrastructure.Data;
using EduGame.Domain.Entities;
using EduGame.Application.Common.Contracts;

namespace EduGame.Infrastructure.Services;

public sealed class NotificationService(
    EduGameDbContext context,
    UserManager<ApplicationUser> userManager)
    : INotificationService
{
    public async Task<IReadOnlyList<NotificationResponseDto>> GetNotificationsAsync(string userId, CancellationToken cancellationToken)
    {
        var notifications = await context.Notifications.AsNoTracking()
            .Where(notification => notification.ReceiverId == userId)
            .OrderByDescending(notification => notification.CreatedAt)
            .Take(100)
            .ToListAsync(cancellationToken);

        if (notifications.Count == 0)
        {
            return Array.Empty<NotificationResponseDto>();
        }

        var senderIds = notifications
            .Select(notification => notification.SenderId)
            .Distinct()
            .ToArray();

        var senders = await userManager.Users
            .Where(user => senderIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, user => user.UserName ?? user.Id, cancellationToken);

        return notifications.Select(notification => new NotificationResponseDto(
            notification.Id,
            senders.GetValueOrDefault(notification.SenderId, "EduGame"),
            notification.Message,
            notification.Type,
            notification.CourseId,
            notification.TargetId,
            notification.IsRead,
            notification.CreatedAt)).ToList();
    }
}