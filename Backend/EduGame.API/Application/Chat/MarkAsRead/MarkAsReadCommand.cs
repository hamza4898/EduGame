using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Chat.MarkAsRead;

public record MarkAsReadCommand(string MessageId, string GroupName) : IRequest<Result<bool>>;