using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Auth.RefreshTokens;

public class RefreshTokensCommandHandler(
    IJwtService jwtService,
    IIdentityRepository identityRepository,
    IAuthService authService
) : IRequestHandler<RefreshTokensCommand, Result<TokenResponseDto>>
{
    public async Task<Result<TokenResponseDto>> Handle(RefreshTokensCommand request, CancellationToken cancellationToken)
    {
        if (request.RefreshToken == null) return Result.Unauthorized("Сессия отсутствует");

        var hashedToken = jwtService.HashToken(request.RefreshToken);

        var storedRefreshToken = await identityRepository.GetStoredRefreshTokenWithUser(hashedToken);

        if (storedRefreshToken == null || storedRefreshToken.User == null)
            return Result.Unauthorized("Сессия недействительна");

        if (storedRefreshToken.IsUsed)
        {
            await identityRepository.RevokeAllRefreshTokensForUser(storedRefreshToken.UserId);
            return Result.Forbidden("Обнаружена попытка повторного использования. Все сессии закрыты");
        }

        if (!storedRefreshToken.IsActive) return Result.Unauthorized("Сессия недействительна");

        storedRefreshToken.IsUsed = true;

        return await authService.GenerateSession(storedRefreshToken.User);         
    }
}