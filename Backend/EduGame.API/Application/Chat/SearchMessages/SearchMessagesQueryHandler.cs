using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using MediatR;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Chat.SearchCommunities;

public sealed class SearchMessagesQueryHandler(ICommunityRepository communityRepository, IIdentityRepository identityRepository)
    : IRequestHandler<SearchMessagesQuery, Result<List<ChatHistoryMessageDto>>>
{
    public async Task<Result<List<ChatHistoryMessageDto>>> Handle(SearchMessagesQuery request, CancellationToken cancellationToken)
    {
        if (request.CommunityId == Guid.Empty) return Result.Invalid(new ValidationError("Укажите сообщество."));

        if (string.IsNullOrWhiteSpace(request.UserId)) return Result.Unauthorized("Не удалось определить пользователя.");

        var searchText = request.SearchText?.Trim();

        if (string.IsNullOrWhiteSpace(searchText) || searchText.Length > 100) return Result.Invalid(new ValidationError("Укажите текст поиска длиной до 100 символов."));

        if (!await communityRepository.IsUserInCommunity(request.CommunityId, request.UserId)) return Result.Forbidden("Вы не состоите в этом сообществе.");

        var community = await communityRepository.GetCommunityByGuid(request.CommunityId);

        if (community is null) return Result.NotFound("Сообщество не найдено.");

        var messages = await communityRepository.SearchMessages(community.Id, searchText, 50, cancellationToken);

        var senderIds = messages.Select(message => message.SenderId).Distinct().ToArray();

        var senderNames = await identityRepository.GetUserNames(senderIds);

        var senderAvatars = await identityRepository.GetAvatarUrls(senderIds);

        var results = messages.Select(message => new ChatHistoryMessageDto(
            message.MessageId,
            message.Text,
            message.MessageType,
            message.SenderId,
            senderNames.GetValueOrDefault(message.SenderId) ?? "Участник",
            new DateTimeOffset(DateTime.SpecifyKind(message.SentAt, DateTimeKind.Utc)),
            senderAvatars.GetValueOrDefault(message.SenderId))).ToList();

        return Result.Success(results);
    }
}