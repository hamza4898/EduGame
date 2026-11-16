using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Chat.SaveMessage;

public record SaveMessageCommand(MessageDto MessageDto) : IRequest<Result<SaveMessageResponseDto>>;