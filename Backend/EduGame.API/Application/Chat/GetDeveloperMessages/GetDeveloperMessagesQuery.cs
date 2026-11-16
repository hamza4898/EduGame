using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Chat.GetDeveloperMessages;

public sealed record GetDeveloperMessagesQuery(string UserId, int Offset, int Limit) : IRequest<Result<List<ChatHistoryMessageDto>>>;