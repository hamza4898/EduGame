using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using EduGame.Domain.Enums;

namespace EduGame.Application.Friends.DeclineFriendRequest;

public class DeclineFriendRequestCommandHandler(IFriendshipRepository repository) : IRequestHandler<DeclineFriendRequestCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(DeclineFriendRequestCommand request, CancellationToken cancellationToken)
    {
        var friendship = await repository.GetById(request.RequestId);

        if (friendship == null) return Result.NotFound("Заявка не найдена");

        if (friendship.ReceiverId != request.UserId) return Result.Forbidden("Вы не имеете права отклонять эту заявку. Она отправлена не вам!");

        if (friendship.Status != FriendshipStatus.Pending) return Result.Error("Эту заявку уже обработали");

        repository.DeleteFriendship(friendship);
        await repository.Save();

        return Result.Success();
    }
}