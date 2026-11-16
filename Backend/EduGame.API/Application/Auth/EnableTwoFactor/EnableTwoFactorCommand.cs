using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Auth.EnableTwoFactor;

public record EnableTwoFactorCommand(string UserId) : IRequest<Result<EnableTwoFactorResponseDto>>;