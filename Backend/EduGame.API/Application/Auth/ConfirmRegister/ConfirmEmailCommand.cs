using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Auth.ConfirmRegister;

public record ConfirmEmailCommand(string Email, string Code) : IRequest<Result<ConfirmEmailResponseDto>>;