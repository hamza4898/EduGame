using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using EduGame.Domain.Enums;

namespace EduGame.Application.Friends.AcceptFriendRequest;

public class AcceptFriendRequestCommandHandler(IFriendshipRepository repository) : IRequestHandler<AcceptFriendRequestCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(AcceptFriendRequestCommand request, CancellationToken cancellationToken)
    {
        var friendship = await repository.GetTrackedFriendshipByIdAsync(request.RequestId, cancellationToken);

        if (friendship == null) return Result.NotFound("Заявка не найдена");

        if (friendship.ReceiverId != request.UserId) return Result.Forbidden("Вы не имеете права принимать эту заявку. Она отправлена не вам!");

        if (friendship.Status != FriendshipStatus.Pending) return Result.Error("Эту заявку уже обработали");

        friendship.Status = FriendshipStatus.Accepted;
        await repository.Save();

        return Result.Success();
    }
}