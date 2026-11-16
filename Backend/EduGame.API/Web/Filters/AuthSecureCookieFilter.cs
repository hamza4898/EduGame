using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Ardalis.Result;
using EduGame.Application.Common.Contracts;
using MediatR;
using EduGame.Application.Auth.ConfirmRegister;
using EduGame.Application.Auth.GoogleLogin;
using EduGame.Domain.Entities;

namespace EduGame.Web.Filters;

public class AuthSecureCookieFilter : IActionFilter
{       
    public void OnActionExecuting(ActionExecutingContext context) {}

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Result is ObjectResult objectResult)
        {
            if (objectResult.Value is Result<LoginResponseDto> loginResult && loginResult.IsSuccess)
            {
                SetTokensToSecureCookies(context, loginResult.Value.AccessToken, loginResult.Value.RefreshToken);
                objectResult.Value = Result.Success(loginResult.Value.IsTwoFactorEnabled);   
            }

            if (objectResult.Value is Result<ConfirmEmailResponseDto> registerResult && registerResult.IsSuccess)
            {
                SetTokensToSecureCookies(context, registerResult.Value.AccessToken, registerResult.Value.RefreshToken);
                objectResult.Value = Result.Success(registerResult.Value.ExternalId);
            }

            if (objectResult.Value is Result<GoogleLoginResponseDto> googleResult && googleResult.IsSuccess)
            {
                SetTokensToSecureCookies(context, googleResult.Value.AccessToken, googleResult.Value.RefreshToken);
            
                objectResult.Value = Result.Success(googleResult.Value.IsProfileComplete);
            }

            if (objectResult.Value is Result<TokenResponseDto> refreshResult && refreshResult.IsSuccess)
            {
                SetTokensToSecureCookies(context, refreshResult.Value.AccessToken, refreshResult.Value.RefreshToken);
                objectResult.Value = Result.Success();
            }

            if (objectResult.Value is Result<Unit> deleteResult && deleteResult.IsSuccess)
            {
                ClearAuthCookies(context.HttpContext);
            }
        }
    }

    private void SetTokensToSecureCookies(ActionExecutedContext context, string accessToken, string refreshToken)
    {
        context.HttpContext.Response.Cookies.Append("EduGame-Access-Token", accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            IsEssential = true,
            Expires = DateTimeOffset.UtcNow.AddMinutes(AuthTokenLifetime.AccessTokenMinutes)
        });

        context.HttpContext.Response.Cookies.Append("EduGame-Refresh-Token", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            IsEssential = true,
            Expires = DateTimeOffset.UtcNow.AddDays(AuthTokenLifetime.RefreshTokenDays)
        });  
    }

    private static void ClearAuthCookies(HttpContext context)
    {
        var expiredCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/"
        };
        context.Response.Cookies.Delete("EduGame-Access-Token", expiredCookieOptions);
        context.Response.Cookies.Delete("EduGame-Refresh-Token", expiredCookieOptions);
    }
}