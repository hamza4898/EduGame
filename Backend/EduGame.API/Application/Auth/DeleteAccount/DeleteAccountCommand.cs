using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Auth.DeleteAccount;

public record DeleteAccountCommand(string UserId, string? Password, string? GoogleIdToken) : IRequest<Result<Unit>>;