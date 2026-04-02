using EduGame.Data;
using EduGame.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using EduGame.Entities;
using Ardalis.Result;
using System.IO.Pipelines;

namespace EduGame.Services
{
    public class LoginService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtService jwtService,
        ILogger<LoginService> logger,
        EduGameIdentityContext eduGameIdentityContext
    ) : ILoginService
    {
        public async Task<Result<TokenResponseDto>> LoginUser(LoginDto loginDto)
        {
            var userLogin = await userManager.FindByEmailAsync(loginDto.Email);

            if (userLogin == null)
            {
                logger.LogWarning("Not found a user with Email: {Email}", loginDto.Email);
                return Result.NotFound("Неверный логин или пароль!");  
            }

            var userSignInResult = await signInManager.CheckPasswordSignInAsync(userLogin, loginDto.Password, lockoutOnFailure: true);

            if (!userSignInResult.Succeeded)
            {
                return userSignInResult switch
                {
                    { IsLockedOut: true } => Result.Forbidden("Слишком много попыток. Попробуйте через 10 минут!"),
                    { IsNotAllowed: true } => Result.Forbidden("Email не подтвержден!"),
                    _ => Result.Unauthorized("Неверный логин или пароль!")
                };    
            }

            return await GenerateTokens(userLogin);
        }

        private async Task<Result<TokenResponseDto>> GenerateTokens(ApplicationUser user)
        {
            var accessToken = jwtService.GenerateAccessToken(user);

            var refreshToken = jwtService.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                HashedToken = refreshToken.HashedRefreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false,
                IsUsed = false
            };

            await eduGameIdentityContext.RefreshTokens.AddAsync(refreshTokenEntity);
            await eduGameIdentityContext.SaveChangesAsync();

            return Result.Success(new TokenResponseDto(accessToken, refreshToken.RefreshToken));
        }

        public async Task<Result<TokenResponseDto>> RefreshTokens(string? token)
        {
            if (token == null) return Result.Unauthorized("Сессия отсутствует!");

            var hashedToken = jwtService.HashToken(token);

            var storedRefreshToken = await eduGameIdentityContext.RefreshTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(rt => rt.HashedToken == hashedToken);

            if (storedRefreshToken == null || storedRefreshToken.User == null || !storedRefreshToken.IsActive)
                return Result.Unauthorized("Сессия недействительна!");

            if (storedRefreshToken.IsUsed)
            {
                await eduGameIdentityContext.RefreshTokens
                        .Where(t => t.UserId == storedRefreshToken.UserId)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(t => t.IsRevoked, true)
                            .SetProperty(t => t.RevokedAt, DateTime.UtcNow));
                        
                return Result.Forbidden("Обнаружена попытка повторного использования. Все сессии закрыты!");
            }

            storedRefreshToken.IsUsed = true;

            return await GenerateTokens(storedRefreshToken.User);
        }
    }
}
