using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Auth.UnlinkGoogleAccount;

public record UnlinkGoogleAccountCommand(string UserId) : IRequest<Result>;