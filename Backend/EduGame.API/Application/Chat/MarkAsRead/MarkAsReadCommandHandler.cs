using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Application.Chat.MarkAsRead;

public class MarkAsReadCommandHandler(IRoomTracker roomTracker) : IRequestHandler<MarkAsReadCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        var currentReadCount = 2;

        await Task.CompletedTask;

        var currentOnlineInRoom = roomTracker.GetOnlineCountInRoom(request.GroupName);

        if (currentReadCount >= currentOnlineInRoom) return Result.Success(true);

        return Result.Success(false);
    }
}