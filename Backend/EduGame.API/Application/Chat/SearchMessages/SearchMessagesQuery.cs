using Ardalis.Result;
using MediatR;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Chat.SearchCommunities;

public sealed record SearchMessagesQuery(Guid CommunityId, string UserId, string SearchText)
    : IRequest<Result<List<ChatHistoryMessageDto>>>;