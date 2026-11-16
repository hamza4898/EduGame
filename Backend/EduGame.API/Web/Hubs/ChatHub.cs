using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using EduGame.Domain.Entities;
using EduGame.Application.Common.Interfaces;
using EduGame.Infrastructure.Data;
using EduGame.Application.Common.Contracts;
using EduGame.Application.Chat.SaveMessage;
using EduGame.Application.Chat.ConfirmDelivery;
using EduGame.Application.Chat.MarkAsRead;
using EduGame.Application.Chat.PublishDeveloperMessage;
using EduGame.Application.Chat.JoinCommunity;
using EduGame.Application.Chat.GetCommunityMembers;
using EduGame.Domain.Enums;

namespace EduGame.Web.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IValidator<Message> _validator;

    private readonly IMapper _mapper;

    private readonly IRoomTracker _roomTracker;

    private readonly IMediator _mediator;

    private readonly ICommunityRepository _repository;

    private readonly IIdentityRepository _identityRepository;

    private readonly EduGameDbContext _context;

    private static string MemberGroup(Guid communityId) => $"community:{communityId:D}";

    public ChatHub(
        IValidator<Message> validator,
        IMapper mapper,
        IRoomTracker roomTracker,
        IMediator mediator,
        ICommunityRepository repository,
        IIdentityRepository identityRepository,
        EduGameDbContext context)
    {
        _validator = validator;
        _mapper = mapper;
        _roomTracker = roomTracker;
        _mediator = mediator;
        _repository = repository;
        _identityRepository = identityRepository;
        _context = context;
    }

    public async Task<bool> SendMessageToGroup(MessageDto messageDto)
    {
        var authenticatedUserId = Context.User?.FindFirst("sub")?.Value 
            ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var authenticatedUserName = Context.User?.Identity?.Name ?? "Неизвестный";

        if (string.IsNullOrEmpty(authenticatedUserId))
        {
            Console.WriteLine($"❌ Ошибка: Соединение {Context.ConnectionId} пытается отправить сообщение без токена авторизации!");
            await Clients.Caller.SendAsync("MessageStatusChanged", messageDto.MessageId, "⚠️", "#ff4d6d", messageDto.CommunityName);
            return false;
        }

        var updatedMessageDto = messageDto with { SenderId = authenticatedUserId };

        if (updatedMessageDto.CommunityName == "EduGame")
        {
            if (Context.User?.IsInRole("admin") != true)
            {
                await Clients.Caller.SendAsync(
                    "MessageStatusChanged",
                    updatedMessageDto.MessageId,
                    "⚠️",
                    "#ff4d6d",
                    updatedMessageDto.CommunityName);
                return false;
            }

            return await PublishDeveloperMessageCore(updatedMessageDto.MessageId, updatedMessageDto.Text, authenticatedUserId);
        }

        var result = await _mediator.Send(new SaveMessageCommand(updatedMessageDto));

        if (!result.IsSuccess)
        {
            Console.WriteLine($"⚠️ Ошибка сохранения пакета [{messageDto.MessageId}]: {string.Join(", ", result.Errors)}");
        
            await Clients.Caller.SendAsync("MessageStatusChanged", messageDto.MessageId, "⚠️", "#ff4d6d", messageDto.CommunityName);
            return false;
        }

        var sentAt = new DateTimeOffset(DateTime.SpecifyKind(
            result.Value.Message.SentAt,
            DateTimeKind.Utc)).ToString("O");
        var senderAvatarUrl = await _context.BaseUsers
            .Where(user => user.Id == authenticatedUserId)
            .Select(user => user.AvatarUrl)
            .FirstOrDefaultAsync();

        var community = Guid.TryParse(updatedMessageDto.CommunityName, out var communityId)
            ? await _repository.GetCommunityByGuid(communityId)
            : await _repository.GetCommunityByName(updatedMessageDto.CommunityName);

        if (community is null)
        {
            await Clients.Caller.SendAsync(
                "MessageStatusChanged",
                updatedMessageDto.MessageId,
                "⚠️",
                "#ff4d6d",
                updatedMessageDto.CommunityName);
            return false;
        }

        await Clients.Caller.SendAsync(
            "MessageStatusChanged",
            updatedMessageDto.MessageId,
            "✓",
            "rgba(255, 255, 255, 0.5)",
            updatedMessageDto.CommunityName,
            sentAt);
        await Clients.Group(MemberGroup(community.CommunityId)).SendAsync(
            "ReceiveMessage",
            updatedMessageDto.MessageId,
            community.CommunityId.ToString(),
            updatedMessageDto.Text,
            authenticatedUserName,
            updatedMessageDto.MessageType,
            Context.ConnectionId,
            authenticatedUserId,
            community.CommunityName,
            sentAt,
            senderAvatarUrl);
        return true;
    }

    [Authorize(Roles = "admin")]
    public async Task<bool> PublishDeveloperMessage(string messageId, string text)
    {
        var senderId = GetAuthenticatedUserId();
        if (string.IsNullOrWhiteSpace(senderId))
            return false;

        return await PublishDeveloperMessageCore(messageId, text, senderId);
    }

    private async Task<bool> PublishDeveloperMessageCore(string messageId, string text, string senderId)
    {
        var result = await _mediator.Send(new PublishDeveloperMessageCommand(senderId, messageId, text));
        if (!result.IsSuccess)
        {
            await Clients.Caller.SendAsync(
                "DeveloperMessageRejected",
                messageId,
                result.Errors.FirstOrDefault() ?? "Сообщение не удалось отправить.");
            return false;
        }

        await Clients.All.SendAsync(
            "ReceiveMessage",
            result.Value.MessageId,
            "EduGame",
            result.Value.Text,
            result.Value.SenderName,
            result.Value.MessageType,
            Context.ConnectionId,
            result.Value.SenderId,
            "EduGame",
            result.Value.SentAt.ToString("O"),
            result.Value.SenderAvatarUrl);
        return true;
    }

    public async Task JoinRoom(string roomName)
    {
        if (string.IsNullOrWhiteSpace(roomName))
            return;

        var authenticatedUserId = Context.User?.FindFirst("sub")?.Value
            ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var communityId = Guid.Empty;
        if (roomName != "EduGame"
            && (!Guid.TryParse(roomName, out communityId)
                || string.IsNullOrWhiteSpace(authenticatedUserId)
                || !await _repository.IsUserInCommunity(communityId, authenticatedUserId)))
        {
            await Clients.Caller.SendAsync("RoomAccessDenied", roomName);
            return;
        }

        if (roomName != "EduGame")
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, MemberGroup(communityId));
        }

        _roomTracker.ConnectionJoined(roomName, Context.ConnectionId);
        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);

        var currentOnline = _roomTracker.GetOnlineCountInRoom(roomName);
        await Clients.Group(roomName).SendAsync("OnlineCountUpdated", roomName, currentOnline);
    }

    public async Task ConfirmDelivery(string messageId, string groupName)
    {
        if (string.IsNullOrWhiteSpace(messageId) || string.IsNullOrWhiteSpace(groupName)) return;

        var result = await _mediator.Send(new ConfirmDeliveryCommand(messageId, groupName));

        if (result.IsSuccess && result.Value)
        {
            await Clients.Group(groupName).SendAsync(
                "MessageStatusChanged",
                messageId,
                "✓✓", 
                "rgba(255, 255, 255, 0.8)",
                groupName
            );

            Console.WriteLine($"✓✓ Две галочки верифицированы сокет-хабом для сообщения [{messageId}]");
        }
    }

    public async Task SendTypingStatus(string roomName, bool isTyping)
    {
        var userName = Context.User?.Identity?.Name ?? "Аноним";
        await Clients.OthersInGroup(roomName).SendAsync("UserTyping", roomName, userName, isTyping);
    }

    public async Task LeaveRoom(string roomName, string userName)
    {
        if (string.IsNullOrWhiteSpace(roomName)) return;

        _roomTracker.ConnectionLeft(roomName, Context.ConnectionId);

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);

        var currentOnline = _roomTracker.GetOnlineCountInRoom(roomName);

        await Clients.Group(roomName).SendAsync("ReceiveSystemNotification", roomName, "", "leave", currentOnline);

        Console.WriteLine($"🚪 Сокет [{Context.ConnectionId}] пользователя {userName} успешно удален из группы {roomName}");
    }

    public async Task MarkMessageAsRead(string messageId, string groupName)
    {
        var result = await _mediator.Send(new MarkAsReadCommand(messageId, groupName));

        if (result.IsSuccess && result.Value)
        {
            await Clients.Group(groupName).SendAsync(
                "MessageStatusChanged", 
                messageId, 
                "✓✓✓", 
                "#4ade80",
                groupName
            );

            Console.WriteLine($"✓✓✓ ТРИ ГАЛОЧКИ верифицированы сокет-хабом для сообщения [{messageId}]");
        }
    }

    public async Task JoinCommunity(string communityName)
    {
        var authenticatedUserId = Context.User?.FindFirst("sub")?.Value 
            ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

        var userName = Context.User?.Identity?.Name;
        var community = Guid.TryParse(communityName, out var communityId)
            ? await _repository.GetCommunityByGuid(communityId)
            : await _repository.GetCommunityByName(communityName);

        if (community is null)
        {
            await Clients.Caller.SendAsync("JoinCommunityError", "Такого сообщества не существует");
            return;
        }

        if (await _repository.IsUserInCommunity(community.CommunityId, authenticatedUserId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, MemberGroup(community.CommunityId));
            await Clients.Caller.SendAsync("JoinCommunitySuccess", communityName);
            return;
        }

        var result = await _mediator.Send(new JoinCommunityCommand(communityName, authenticatedUserId, userName!));

        if (!result.IsSuccess)
        {
            var errorReason = result.Errors.FirstOrDefault() ?? "Ошибка вступления";
        
            await Clients.Caller.SendAsync("JoinCommunityError", errorReason);
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, MemberGroup(community.CommunityId));

        await Clients.Caller.SendAsync("JoinCommunitySuccess", communityName);
        await Clients.Group(MemberGroup(community.CommunityId)).SendAsync("CommunityMembersChanged", community.CommunityId);
    }

    public async Task GetCommunityMembers(Guid communityId, int offset, int limit)
    {
        var authenticatedUserId = Context.User?.FindFirst("sub")?.Value 
            ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(authenticatedUserId))
        {
            await Clients.Caller.SendAsync("GetMembersError", "Не удалось определить пользователя.");
            return;
        }

        var result = await _mediator.Send(new GetCommunityMembersQuery(communityId, authenticatedUserId, offset, limit));

        if (!result.IsSuccess)
        {
            var errorReason = result.Errors.FirstOrDefault() ?? "Доступ к списку участников ограничен.";
            await Clients.Caller.SendAsync("GetMembersError", errorReason);
            return;
        }

        var currentUserMember = await _repository.GetCommunityMember(communityId, authenticatedUserId);

        var myRoleNumber = currentUserMember != null ? (int)currentUserMember.Role : 0;

        await Clients.Caller.SendAsync("ReceiveMembersPortion", new {
            communityId = communityId.ToString(),
            members = result.Value, 
            myRole = myRoleNumber,
            offset = offset
        });
    }

    public async Task ChangePlayerRole(Guid communityId, string targetUserId, int role)
    {
        var actorId = GetAuthenticatedUserId();
        if (string.IsNullOrWhiteSpace(actorId) || communityId == Guid.Empty
            || string.IsNullOrWhiteSpace(targetUserId) || role is not (0 or 1))
        {
            await Clients.Caller.SendAsync("CommunityActionError", "Некорректные данные для смены роли.");
            return;
        }

        var actor = await _context.CommunityMembers.FirstOrDefaultAsync(member =>
            member.CommunityId == communityId && member.UserId == actorId);
        var target = await _context.CommunityMembers.FirstOrDefaultAsync(member =>
            member.CommunityId == communityId && member.UserId == targetUserId);
        if (actor?.Role != ClanRole.Leader || target is null || target.UserId == actorId || target.Role == ClanRole.Leader)
        {
            await Clients.Caller.SendAsync("CommunityActionError", "Только создатель может менять роль участника; создателя изменить нельзя.");
            return;
        }

        target.Role = (ClanRole)role;
        await _context.SaveChangesAsync();
        await Clients.Group(MemberGroup(communityId)).SendAsync("CommunityMembersChanged", communityId);
    }

    public async Task KickPlayerFromCommunity(Guid communityId, string targetUserId)
    {
        var actorId = GetAuthenticatedUserId();
        if (string.IsNullOrWhiteSpace(actorId) || communityId == Guid.Empty
            || string.IsNullOrWhiteSpace(targetUserId))
        {
            await Clients.Caller.SendAsync("CommunityActionError", "Некорректные данные для исключения участника.");
            return;
        }

        var actor = await _context.CommunityMembers.AsNoTracking().FirstOrDefaultAsync(member =>
            member.CommunityId == communityId && member.UserId == actorId);
        var target = await _context.CommunityMembers.FirstOrDefaultAsync(member =>
            member.CommunityId == communityId && member.UserId == targetUserId);
        if (actor is null || actor.Role == ClanRole.Member || target is null
            || target.UserId == actorId || target.Role == ClanRole.Leader
            || (actor.Role == ClanRole.Officer && target.Role != ClanRole.Member))
        {
            await Clients.Caller.SendAsync("CommunityActionError", "Недостаточно прав для исключения этого участника.");
            return;
        }

        var community = await _context.Communities.FirstOrDefaultAsync(item => item.CommunityId == communityId);
        if (community is null)
        {
            await Clients.Caller.SendAsync("CommunityActionError", "Сообщество не найдено.");
            return;
        }

        _context.CommunityMembers.Remove(target);
        community.MembersCount = Math.Max(0, community.MembersCount - 1);
        await _context.SaveChangesAsync();

        await Clients.User(targetUserId).SendAsync("RemovedFromCommunity", communityId);
        await Clients.Group(MemberGroup(communityId)).SendAsync("CommunityMembersChanged", communityId);
    }

    public async Task LeaveCommunity(Guid communityId)
    {
        if (communityId == Guid.Empty)
            return;

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, MemberGroup(communityId));
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, communityId.ToString());
    }

    private string? GetAuthenticatedUserId() =>
        Context.User?.FindFirst("sub")?.Value
        ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public async Task GetOnlineCount(string roomName)
    {
        var count = _roomTracker.GetOnlineCountInRoom(roomName);
        await Clients.Caller.SendAsync("OnlineCountUpdated", roomName, count);
    }

    public override async Task OnConnectedAsync()
    {
        var authenticatedUserId = Context.User?.FindFirst("sub")?.Value
            ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrWhiteSpace(authenticatedUserId))
        {
            var communities = await _repository.GetUserCommunities(authenticatedUserId);
            foreach (var community in communities)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, MemberGroup(community.CommunityId));
            }
        }

        Console.WriteLine($"🔌 Новый сокет пробил туннель к серверу! ConnectionId: [{Context.ConnectionId}]");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"❌ Сокет [{Context.ConnectionId}] отключился от сервера. Причина ошибки: {exception?.Message ?? "Плановый выход"}");
        _roomTracker.RemoveConnectionFully(Context.ConnectionId);

        await base.OnDisconnectedAsync(exception);
    }
}