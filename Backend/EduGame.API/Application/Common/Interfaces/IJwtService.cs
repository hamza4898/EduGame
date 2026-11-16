using EduGame.Domain.Entities;
using EduGame.Application.Auth.RefreshTokens;

namespace EduGame.Application.Common.Interfaces;

public interface IJwtService
{
    Task<string> GenerateAccessToken(ApplicationUser user);

    RefreshTokenResponseDto GenerateRefreshToken();

    string HashToken(string token);
}