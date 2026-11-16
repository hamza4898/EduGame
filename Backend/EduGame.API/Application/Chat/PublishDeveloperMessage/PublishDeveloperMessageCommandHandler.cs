using Ardalis.Result;
using EduGame.Domain.Entities;
using EduGame.Domain.Enums;
using EduGame.Application.Common.Interfaces;
using MediatR;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Chat.PublishDeveloperMessage;

public sealed class PublishDeveloperMessageCommandHandler(IRepository<Message> messageRepository, IIdentityRepository identityRepository)
    : IRequestHandler<PublishDeveloperMessageCommand, Result<ChatHistoryMessageDto>>
{
    public async Task<Result<ChatHistoryMessageDto>> Handle(PublishDeveloperMessageCommand request, CancellationToken cancellationToken)
    {
        var text = request.Text.Trim();

        if (string.IsNullOrWhiteSpace(request.SenderId)
            || !Guid.TryParse(request.MessageId, out _)
            || text.Length is 0 or > 1000)
            return Result.Invalid(new ValidationError("Message", "Сообщение должно содержать от 1 до 1000 символов."));

        var message = new Message
        {
            MessageId = request.MessageId,
            Text = text,
            MessageType = "text",
            SentAt = DateTime.UtcNow,
            SenderId = request.SenderId,
            Status = MessageStatus.SentToServer,
            CommunityId = null
        };

        await messageRepository.Add(message);
        await messageRepository.Save();

        var senderNames = await identityRepository.GetUserNames([request.SenderId]);

        var senderAvatars = await identityRepository.GetAvatarUrls([request.SenderId]);

        return Result.Success(new ChatHistoryMessageDto(
            message.MessageId,
            message.Text,
            message.MessageType,
            message.SenderId,
            senderNames.GetValueOrDefault(request.SenderId) ?? "Разработчик",
            new DateTimeOffset(DateTime.SpecifyKind(message.SentAt, DateTimeKind.Utc)),
            senderAvatars.GetValueOrDefault(request.SenderId)));
    }
}