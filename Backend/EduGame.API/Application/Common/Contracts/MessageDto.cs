namespace EduGame.Application.Common.Contracts;

public record MessageDto(
    string MessageId,
    string CommunityName,
    string SenderId,
    string Text,
    string MessageType
);