using Ardalis.Result;
using EduGame.DTOs;

namespace EduGame.Services
{
    public interface ILoginService
    {
        Task<Result<TokenResponseDto>> LoginUser(LoginDto loginDto);

        Task<Result<TokenResponseDto>> RefreshTokens(string? token);
    }
}