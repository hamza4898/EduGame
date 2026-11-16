using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using MediatR;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Chat.GetMessages;

public class GetMessagesQueryHandler(ICommunityRepository communityRepository, IIdentityRepository identityRepository)
    : IRequestHandler<GetMessagesQuery, Result<List<ChatHistoryMessageDto>>>
{
    public async Task<Result<List<ChatHistoryMessageDto>>> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
    {
        if (request.CommunityId == Guid.Empty) return Result.Invalid(new ValidationError("Идентификатор сообщества обязателен."));

        if (string.IsNullOrWhiteSpace(request.UserId)) return Result.Unauthorized("Не удалось определить пользователя.");

        var community = await communityRepository.GetCommunityByGuid(request.CommunityId);

        if (community is null) return Result.NotFound("Сообщество не найдено.");

        if (!await communityRepository.IsUserInCommunity(request.CommunityId, request.UserId)) return Result.Forbidden("Вы не состоите в этом сообществе.");

        var offset = Math.Max(0, request.Offset);

        var limit = request.Limit <= 0 ? 50 : Math.Min(request.Limit, 50);

        var messages = await communityRepository.GetMessagesByCommunityId(community.Id, offset, limit, request.Since?.ToUniversalTime());

        var senderNames = await identityRepository.GetUserNames(
            messages.Select(message => message.SenderId).Distinct().ToArray());

        var senderAvatars = await identityRepository.GetAvatarUrls(
            messages.Select(message => message.SenderId).Distinct().ToArray());

        var history = messages.Select(message => new ChatHistoryMessageDto(
            message.MessageId,
            message.Text,
            message.MessageType,
            message.SenderId,
            senderNames.GetValueOrDefault(message.SenderId) ?? "Участник",
            new DateTimeOffset(DateTime.SpecifyKind(message.SentAt, DateTimeKind.Utc)),
            senderAvatars.GetValueOrDefault(message.SenderId))).ToList();

        return Result.Success(history);
    }
}