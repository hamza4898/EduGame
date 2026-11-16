using Ardalis.Result;
using MediatR;

namespace EduGame.Application.Auth.DisableTwoFactor;

public record DisableTwoFactorCommand(string UserId, string Code) : IRequest<Result<Unit>>;