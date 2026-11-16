using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Application.Auth.Logout;

public class LogoutCommandHandler(IIdentityRepository identityRepository) : IRequestHandler<LogoutCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        await identityRepository.RevokeAllRefreshTokensForUser(request.UserId);

        return Result.Success();
    }
}