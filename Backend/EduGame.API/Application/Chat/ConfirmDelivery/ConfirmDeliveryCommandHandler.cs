using EduGame.Application.Common.Interfaces;
using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Chat.ConfirmDelivery;

public class ConfirmDeliveryCommandHandler(IRoomTracker roomTracker) : IRequestHandler<ConfirmDeliveryCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ConfirmDeliveryCommand request, CancellationToken cancellationToken)
    {
        var currentDeliveredCount = 2;

        var currentOnlineInRoom = roomTracker.GetOnlineCountInRoom(request.GroupName);

        if (currentDeliveredCount >= currentOnlineInRoom)
            return Result.Success(true);

        await Task.CompletedTask;

        return Result.Success(false);
    }
} 