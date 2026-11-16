namespace EduGame.Application.Common.Contracts;

public sealed record ChatHistoryMessageDto(
    string MessageId,
    string Text,
    string MessageType,
    string SenderId,
    string SenderName,
    DateTimeOffset SentAt,
    string? SenderAvatarUrl);