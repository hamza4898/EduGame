using Ardalis.Result;
using NanoidDotNet;
using EduGame.Application.Common.Interfaces;
using EduGame.Application.Common.Contracts;
using EduGame.Domain.Entities;

namespace EduGame.Infrastructure.Services;

public class AuthService(
    IJwtService jwtService,
    IIdentityRepository identityRepository
) : IAuthService
{
    public async Task<TokenResponseDto> GenerateSession(ApplicationUser user)
    {
        var accessToken = await jwtService.GenerateAccessToken(user);

        var refreshToken = jwtService.GenerateRefreshToken();

        var refreshTokenEntity = new RefreshToken
        {
            HashedToken = refreshToken.HashedRefreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(AuthTokenLifetime.RefreshTokenDays),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false,
            IsUsed = false       
        };

        await identityRepository.AddRefreshToken(refreshTokenEntity);

        await identityRepository.Save(); 

        return Result.Success(new TokenResponseDto(accessToken, refreshToken.RefreshToken));
    }

    public async Task<ResetCodeResultDto> GeneratePasswordResetCode(ApplicationUser user)
    {
        await identityRepository.DeactivateUnusedResetCodes(user.Id);

        var code = Nanoid.Generate(Nanoid.Alphabets.UppercaseLettersAndDigits, 6);

        var hashedCode = BCrypt.Net.BCrypt.HashPassword(code);

        var codeEntity = new PasswordResetCode
        {
            UserId = user.Id,
            HashedCode = hashedCode,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        await identityRepository.AddPasswordResetCode(codeEntity);
        await identityRepository.Save();

        return new ResetCodeResultDto(codeEntity.ExternalId, code);
    }
}