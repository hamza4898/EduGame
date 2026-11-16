using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Application.Notifications.MarkNotificationRead;

public sealed class MarkNotificationReadCommandHandler(ICourseManagementService service)
    : IRequestHandler<MarkNotificationReadCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        if (request.NotificationId <= 0) return Result.Invalid(new ValidationError("NotificationId", "Идентификатор уведомления невалиден."));

        await service.MarkNotificationReadAsync(request.UserId, request.NotificationId, cancellationToken);
        
        return Result.Success(true);
    }
}