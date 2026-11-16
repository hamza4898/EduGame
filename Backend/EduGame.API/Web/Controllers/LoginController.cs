using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MediatR;
using EduGame.Application.Auth.Login;
using EduGame.Application.Auth.GoogleLogin;
using EduGame.Application.Auth.Logout;
using EduGame.Web.Filters;

namespace EduGame.Web.Controllers;

[ApiController]
[Route("api/login/[action]")]
public class LoginController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    [ActionName("LoginUser")]
    [TypeFilter(typeof(AuthSecureCookieFilter))]
    public async Task<IActionResult> LoginUser([FromBody] LoginDto loginDto)
    {
        var result = await mediator.Send(new LoginUserCommand(loginDto.Email, loginDto.Password));

        return Ok(result);
    }

    [HttpPost]
    [AllowAnonymous]
    [ActionName("GoogleLogin")]
    [TypeFilter(typeof(AuthSecureCookieFilter))]
    public async Task<IActionResult> GoogleLogin(GoogleLoginDto dto)
    {       
        var result = await mediator.Send(new GoogleLoginCommand(dto.IdToken));

        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    [ActionName("Logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value!; 

        var result = await mediator.Send(new LogoutCommand(userId));

        var expiredCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/"
        };
        
        Response.Cookies.Delete("EduGame-Access-Token", expiredCookieOptions);
        Response.Cookies.Delete("EduGame-Refresh-Token", expiredCookieOptions);

        return Ok(result);   
    }
}