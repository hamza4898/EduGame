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
        ILogger<LoginService> logger
    ) : ILoginService
    {
        public async Task<Result<string>> LoginUser(LoginDto loginDto)
        {
            var userLogin = await userManager.FindByEmailAsync(loginDto.Email);

            if (userLogin == null)
            {
                logger.LogWarning("Not found a user with Email: {Email}", loginDto.Email);
                return Result.NotFound("Неверный логин или пароль!");  
            }

            var userSignInResult = await signInManager.CheckPasswordSignInAsync(userLogin, loginDto.Password, lockoutOnFailure: true);

            return userSignInResult switch
            {
                { Succeeded: true } => jwtService.GenerateToken(userLogin),
                { IsLockedOut: true } => Result.Forbidden("Слишком много попыток. Попробуйте через 10 минут!"),
                { IsNotAllowed: true } => Result.Forbidden("Email не подтвержден!"),
                _ => Result.Unauthorized("Неверный логин или пароль!")
            };
        }
    }
}
