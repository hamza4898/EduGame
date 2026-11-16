using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Chat.SupportAiChat;

public record GetAiSupportQuery(string UserId, string UserPrompt) : IRequest<Result<AiSupportMessageResponseDto>>;