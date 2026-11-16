using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Auth.ForgotPassword;

public record ForgotPasswordCommand(string Email, string ClientRoute) : IRequest<Result<Guid>>;