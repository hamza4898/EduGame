using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Courses.GetMyNotifications;

public sealed class GetMyNotificationsQueryHandler(
    INotificationService notificationService,
    IUserRepository userRepository) 
    : IRequestHandler<GetMyNotificationsQuery, Result<List<NotificationResponseDto>>>
{
    public async Task<Result<List<NotificationResponseDto>>> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId)) 
            return Result.Unauthorized("Не удалось определить пользователя.");

        var userExists = await userRepository.ExistsAsync(request.UserId, cancellationToken);

        if (!userExists) return Result.NotFound("Профиль пользователя не найден.");

        var notifications = await notificationService.GetNotificationsAsync(request.UserId, cancellationToken);

        return Result.Success(notifications.ToList());
    }
}