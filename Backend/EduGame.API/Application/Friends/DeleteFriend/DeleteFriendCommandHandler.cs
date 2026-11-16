using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using EduGame.Domain.Enums;

namespace EduGame.Application.Friends.DeleteFriend;

public class DeleteFriendCommandHandler(IFriendshipRepository repository) : IRequestHandler<DeleteFriendCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(DeleteFriendCommand request, CancellationToken cancellationToken)
    {
        var friendship = await repository.GetById(request.RequestId);

        if (friendship == null) return Result.NotFound("Заявка не найдена");

        if (friendship.SenderId != request.UserId && friendship.ReceiverId != request.UserId) 
            return Result.Forbidden("Вы не имеете доступа к этому действию");

        if (friendship.Status != FriendshipStatus.Accepted) return Result.Error("Данный юзер не находится в друзьях");

        repository.DeleteFriendship(friendship);
        await repository.Save();

        return Result.Success();
    }
}