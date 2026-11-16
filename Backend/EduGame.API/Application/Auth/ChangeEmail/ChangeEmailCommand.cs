using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Auth.ChangeEmail;

public record ChangeEmailCommand(string UserId, string CurrentPassword, string NewEmail) : IRequest<Result<string>>;