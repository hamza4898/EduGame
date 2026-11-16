using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Auth.VerifyCode;

public record VerifyCodeCommand(Guid SessionId, string Code) : IRequest<Result<Unit>>;