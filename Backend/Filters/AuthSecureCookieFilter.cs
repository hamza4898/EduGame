using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.IO.Pipelines;
using System.Security;
using System.Security.Cryptography;
using Ardalis.Result;
using EduGame.DTOs;

namespace EduGame.Filters
{
    public class AuthSecureCookieFilter : IActionFilter
    {       
        public void OnActionExecuting(ActionExecutingContext context) {}

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Result is ObjectResult objectResult && objectResult.Value is Result<TokenResponseDto> result)
            {
                if (!result.IsSuccess) return;
                
                var tokenData = result.Value;

                context.HttpContext.Response.Cookies.Append("EduGame-Access-Token", tokenData.AccessToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddMinutes(15)
                });

                context.HttpContext.Response.Cookies.Append("EduGame-Refresh-Token", tokenData.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(7)
                });

                objectResult.Value = Result.Success("Авторизация в EduGame прошла успешно!");
            }
        }
    }
}