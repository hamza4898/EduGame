using EduGame.Infrastructure.Data;
using EduGame.Domain.Entities;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Infrastructure.Repositories;

public sealed class NotificationRepository(EduGameDbContext context) : INotificationRepository
{
    public async Task<int> AddBroadcastAsync(
        IReadOnlyCollection<Notification> notifications,
        CancellationToken cancellationToken)
    {
        await context.Notifications.AddRangeAsync(notifications, cancellationToken);
        return await context.SaveChangesAsync(cancellationToken);
    }
}