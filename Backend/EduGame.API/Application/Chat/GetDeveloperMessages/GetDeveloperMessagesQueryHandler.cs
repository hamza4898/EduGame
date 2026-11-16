using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using MediatR;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Chat.GetDeveloperMessages;

public sealed class GetDeveloperMessagesQueryHandler(
    ICommunityRepository communityRepository,
    IIdentityRepository identityRepository)
    : IRequestHandler<GetDeveloperMessagesQuery, Result<List<ChatHistoryMessageDto>>>
{
    public async Task<Result<List<ChatHistoryMessageDto>>> Handle(GetDeveloperMessagesQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId)) return Result.Unauthorized("Не удалось определить пользователя.");

        var messages = await communityRepository.GetDeveloperMessages(
            Math.Max(0, request.Offset),
            request.Limit <= 0 ? 50 : Math.Min(request.Limit, 50),
            cancellationToken);

        var senderIds = messages.Select(message => message.SenderId).Distinct().ToArray();

        var senderNames = await identityRepository.GetUserNames(senderIds);

        var senderAvatars = await identityRepository.GetAvatarUrls(senderIds);

        return Result.Success(messages.Select(message => new ChatHistoryMessageDto(
            message.MessageId,
            message.Text,
            message.MessageType,
            message.SenderId,
            senderNames.GetValueOrDefault(message.SenderId) ?? "Разработчик",
            new DateTimeOffset(DateTime.SpecifyKind(message.SentAt, DateTimeKind.Utc)),
            senderAvatars.GetValueOrDefault(message.SenderId))).ToList());
    }
}