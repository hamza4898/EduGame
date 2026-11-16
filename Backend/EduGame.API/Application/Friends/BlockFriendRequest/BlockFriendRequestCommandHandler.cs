using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using EduGame.Domain.Enums;

namespace EduGame.Application.Friends.BlockFriendRequest;

public class BlockFriendRequestCommandHandler(IFriendshipRepository repository) : IRequestHandler<BlockFriendRequestCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(BlockFriendRequestCommand request, CancellationToken cancellationToken)
    {
        var friendship = await repository.GetTrackedFriendshipByIdAsync(request.RequestId, cancellationToken);

        if (friendship == null) return Result.NotFound("Заявка не найдена");

        if (friendship.SenderId != request.UserId && friendship.ReceiverId != request.UserId) 
            return Result.Forbidden("Вы не имеете доступа к этому");

        if (friendship.Status == FriendshipStatus.Blocked)
            return Result.Error("Пользователь уже заблокирован.");

        friendship.Status = FriendshipStatus.Blocked;
        await repository.Save();

        return Result.Success();
    }
}