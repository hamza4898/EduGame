using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Auth.LinkGoogleToCurrentAccount;

public record LinkGoogleAccountCommand(string IdToken, string CurrentUserId) : IRequest<Result>;