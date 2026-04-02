using Microsoft.AspNetCore.Mvc;
using EduGame.Services;
using EduGame.DTOs;
using Ardalis.Result;
using EduGame.Filters;

namespace EduGame.Controllers
{
    [ApiController]
    [Route("api/login/[action]")]
    public class LoginController : ControllerBase
    {
        private readonly ILoginService _loginService;

        public LoginController(ILoginService loginService) => _loginService = loginService;

        [HttpPost]
        [ActionName("LoginUser")]
        [TypeFilter(typeof(AuthSecureCookieFilter))]
        public async Task<IActionResult> LoginUser(LoginDto loginDto)
        {
            var user = await _loginService.LoginUser(loginDto);

            return Ok(user);
        }

        [HttpPost]
        [ActionName("RefreshTokens")]
        [TypeFilter(typeof(AuthSecureCookieFilter))]
        public async Task<IActionResult> RefreshTokens()
        {
            var refreshToken = Request.Cookies["EduGame-Refresh-Token"];

            var newTokens = await _loginService.RefreshTokens(refreshToken);

            return Ok(newTokens);
        }
    }
}
