using MediatR;
using Ardalis.Result;
using Google.Apis.Auth;
using EduGame.Application.Common.Interfaces;
using EduGame.Domain.Entities;
using EduGame.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace EduGame.Application.Auth.GoogleLogin;

public class GoogleLoginCommandHandler(
    IConfiguration configuration,
    IIdentityService identityService,
    IAuthService authService
) : IRequestHandler<GoogleLoginCommand, Result<GoogleLoginResponseDto>>
{
    public async Task<Result<GoogleLoginResponseDto>> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = new[] { configuration["Authentication:Google:ClientId"] }
        };

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
        }
        catch (InvalidJwtException)
        {
            return Result.Invalid(new ValidationError { ErrorMessage = "Невалидный Google токен" });
        }

        var user = await identityService.FindByEmailAsync(payload.Email);

        if (user == null)
        {
            var newUser = new ApplicationUser
            {
                UserName = payload.Email,
                Email = payload.Email,
                EmailConfirmed = true, 
                IsProfileComplete = false, 
                UserRole = UserRole.Student
            };

            var createResult = await identityService.CreateAsync(newUser); 
            if (!createResult.Succeeded)
            {
                return Result.Error("Не удалось зарегистрировать пользователя");
            }

            var loginInfo = new UserLoginInfo("Google", payload.Subject, "Google");
            await identityService.AddLogin(newUser, loginInfo);

            user = newUser;
        }
        else
        {
            var userLogins = await identityService.GetLoginsAsync(user);
            var hasGoogleLogin = userLogins.Any(l => l.LoginProvider == "Google");

            if (!hasGoogleLogin)
            {
                var loginInfo = new UserLoginInfo("Google", payload.Subject, "Google");
                await identityService.AddLogin(user, loginInfo);
            }
        }

        var tokens = await authService.GenerateSession(user);

        var response = new GoogleLoginResponseDto(
            tokens.AccessToken,
            tokens.RefreshToken,
            user.IsProfileComplete
        );

        return Result.Success(response);
    }
}