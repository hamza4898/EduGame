using EduGame.Domain.Entities;

namespace EduGame.Application.Common.Interfaces;

public interface INotificationRepository
{
    Task<int> AddBroadcastAsync(IReadOnlyCollection<Notification> notifications, CancellationToken cancellationToken);
}