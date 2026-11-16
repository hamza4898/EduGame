using MediatR;
using Ardalis.Result;
using EduGame.Web.Hubs;
using Microsoft.AspNetCore.SignalR;
using EduGame.Application.Common.Interfaces;
using EduGame.Domain.Entities;
using EduGame.Domain.Enums;

namespace EduGame.Application.Friends.SendFriendRequest;

public class SendFriendRequestCommandHandler(
    IFriendshipRepository friendshipRepository,
    IUserRepository userRepository,
    IHubContext<NotificationHub> hubContext) : IRequestHandler<SendFriendRequestCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(SendFriendRequestCommand request, CancellationToken cancellationToken)
    {
        if (request.SenderId == request.ReceiverId) return Result.Invalid(new ValidationError("Вы не можете отправить заявку самому себе"));

        var alreadyExists = await friendshipRepository.FriendshipExistsAsync(request.SenderId, request.ReceiverId, cancellationToken);

        if (alreadyExists) return Result.Conflict("Заявка уже отправлена или вы уже друзья");

        if (!await userRepository.ExistsAsync(request.SenderId, cancellationToken)) return Result.NotFound("Отправитель не найден.");
            
        if (!await userRepository.ExistsAsync(request.ReceiverId, cancellationToken)) return Result.NotFound("Получатель не найден.");

        var friendship = new Friendship
        {
            SenderId = request.SenderId,
            ReceiverId = request.ReceiverId,
            Status = FriendshipStatus.Pending
        };

        var senderName = await userRepository.GetUserNameAsync(request.SenderId, cancellationToken);
        
        var notification = new Notification
        {
            SenderId = request.SenderId,
            ReceiverId = request.ReceiverId,
            Message = $"{senderName ?? "Пользователь"} отправил вам заявку в друзья.",
            Type = "FriendRequest",
            CreatedAt = DateTime.UtcNow
        };

        friendshipRepository.AddFriendship(friendship);
        friendshipRepository.AddNotification(notification);
        await friendshipRepository.Save();

        await hubContext.Clients.User(request.ReceiverId).SendAsync(
            "ReceiveNotification",
            new
            {
                id = notification.Id,
                message = notification.Message,
                type = notification.Type,
                createdAt = notification.CreatedAt
            },
            cancellationToken);

        return Result.Success(Unit.Value);
    }
}