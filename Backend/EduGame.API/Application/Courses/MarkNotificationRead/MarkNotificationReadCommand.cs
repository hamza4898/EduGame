using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Notifications.MarkNotificationRead;

public sealed record MarkNotificationReadCommand(
    string UserId,
    int NotificationId) : IRequest<Result<bool>>;