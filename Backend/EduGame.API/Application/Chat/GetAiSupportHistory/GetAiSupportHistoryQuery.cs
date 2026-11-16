using Ardalis.Result;
using MediatR;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Chat.GetAiSupportHistory;

public sealed record GetAiSupportHistoryQuery(string UserId) : IRequest<Result<List<AiSupportMessageResponseDto>>>;