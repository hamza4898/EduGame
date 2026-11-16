using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Auth.Logout;

public record LogoutCommand(string UserId) : IRequest<Result<Unit>>;