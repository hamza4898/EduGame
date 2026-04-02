using Ardalis.Result;
using EduGame.Entities;
using EduGame.DTOs;

namespace EduGame.Services
{
    public interface IJwtService
    {
        string GenerateAccessToken(ApplicationUser user);

        RefreshTokenResponseDto GenerateRefreshToken();

        string HashToken(string token);
    }
}