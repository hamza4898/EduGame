using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Chat.GetMessages;

public record GetMessagesQuery(Guid CommunityId, string UserId, int Offset, int Limit, DateTime? Since = null)
    : IRequest<Result<List<ChatHistoryMessageDto>>>;