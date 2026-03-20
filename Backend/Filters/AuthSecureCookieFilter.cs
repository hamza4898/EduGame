using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.IO.Pipelines;
using System.Security;
using System.Security.Cryptography;
using Ardalis.Result;

namespace EduGame.Filters
{
    public class AuthSecureCookieFilter : IActionFilter
    {
        private readonly ILogger<AuthSecureCookieFilter> _logger;

        public AuthSecureCookieFilter(ILogger<AuthSecureCookieFilter> logger)
        {
            _logger = logger;
        }
        
        public void OnActionExecuting(ActionExecutingContext context) {}

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Result is ObjectResult objectResult && objectResult.Value is Result<string> objectValue)
            {
                if (!objectValue.IsSuccess) return;
                
                string token = objectValue.Value;

                context.HttpContext.Response.Cookies.Append("EduGame-Access-Token", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddMinutes(15)
                });

                objectResult.Value = Result.Success("Авторизация в EduGame прошла успешно!");
            }
        }
    }
}