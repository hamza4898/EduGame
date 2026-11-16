using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Chat.PublishDeveloperMessage;

public sealed record PublishDeveloperMessageCommand(
    string SenderId,
    string MessageId,
    string Text) : IRequest<Result<ChatHistoryMessageDto>>;