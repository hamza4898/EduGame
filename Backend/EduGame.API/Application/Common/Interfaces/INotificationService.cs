using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Common.Interfaces;

public interface INotificationService
{
    Task<IReadOnlyList<NotificationResponseDto>> GetNotificationsAsync(string userId, CancellationToken cancellationToken);
}