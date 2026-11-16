using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Auth.VerifyTwoFactor;

public record VerifyTwoFactorCommand(string UserId, string Code) : IRequest<Result<Unit>>;