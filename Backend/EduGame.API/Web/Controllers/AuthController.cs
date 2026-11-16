using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using EduGame.Web.Filters;
using EduGame.Application.Auth.ConfirmRegister;
using EduGame.Application.Auth.RefreshTokens;
using EduGame.Application.Common.Contracts;
using EduGame.Application.Auth.ForgotPassword;
using EduGame.Application.Auth.VerifyCode;
using EduGame.Application.Auth.ChangePassword;
using EduGame.Application.Auth.EnableTwoFactor;
using EduGame.Application.Auth.VerifyTwoFactor;
using EduGame.Application.Auth.DisableTwoFactor;
using EduGame.Application.Auth.ChangeEmail;
using EduGame.Application.Auth.UnlinkGoogleAccount;
using EduGame.Application.Auth.DeleteAccount;

namespace EduGame.Web.Controllers;

[ApiController]
[Route("api/auth/[action]")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ActionName("ConfirmEmail")]
    [TypeFilter(typeof(AuthSecureCookieFilter))]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDto dto)
    {
        var result = await mediator.Send(new ConfirmEmailCommand(dto.Email, dto.Code));

        return Ok(result);
    }

    [HttpPost]
    [ActionName("refreshTokens")]
    [TypeFilter(typeof(AuthSecureCookieFilter))]
    [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> RefreshTokens(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies["EduGame-Refresh-Token"];

        var result = await mediator.Send(new RefreshTokensCommand(refreshToken), cancellationToken);

        return Ok(result);
    }

    [HttpPost]
    [ActionName("forgotPassword")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
    {
        var result = await mediator.Send(new ForgotPasswordCommand(forgotPasswordDto.Email, forgotPasswordDto.ClientRoute));

        return Accepted(result);
    }

    [HttpPost]
    [ActionName("checkVerifyCode")] 
    public async Task<IActionResult> CheckVerifyCode([FromBody] VerifyCodeDto verifyCodeDto)
    {
        var result = await mediator.Send(new VerifyCodeCommand(verifyCodeDto.SessionId, verifyCodeDto.Code));

        return Ok(result);
    }

    [HttpPost]
    [ActionName("changePassword")] 
    [TypeFilter(typeof(AuthSecureCookieFilter))]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        var result = await mediator.Send(new ChangePasswordCommand(changePasswordDto.SessionId, changePasswordDto.NewPassword));

        return Ok(result);      
    }

    [HttpPost]
    [Authorize]
    [ActionName("EnableTwoFactorSecurity")]
    public async Task<IActionResult> EnableTwoFactorSecurity()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value!; 

        var result = await mediator.Send(new EnableTwoFactorCommand(userId));

        return Ok(result);   
    }

    [HttpPost]
    [Authorize]
    [ActionName("VerifyTwoFactorSecurity")]
    public async Task<IActionResult> VerifyTwoFactorSecurity(VerifyTwoFactorDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value!;

        var result = await mediator.Send(new VerifyTwoFactorCommand(userId, dto.Code));

        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    [ActionName("DisableTwoFactorSecurity")]
    public async Task<IActionResult> DisableTwoFactorSecurity(DisableTwoFactorDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value!;

        var result = await mediator.Send(new DisableTwoFactorCommand(userId, dto.Code));

        return Ok(result);    
    }

    [HttpPost]
    [Authorize]
    [ActionName("ChangeEmail")]
    public async Task<IActionResult> ChangeEmail(ChangeEmailDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value!;

        var result = await mediator.Send(new ChangeEmailCommand(userId, dto.CurrentPassword, dto.NewEmail));

        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    [ActionName("UnlinkGoogleAccount")]
    public async Task<IActionResult> UnlinkGoogleAccount()
    {   
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value!;

        var result = await mediator.Send(new UnlinkGoogleAccountCommand(userId));

        return Ok(result);
    }   

    [HttpPost]
    [Authorize]
    [ActionName("DeleteAccount")]
    [TypeFilter(typeof(AuthSecureCookieFilter))]
    public async Task<IActionResult> DeleteAccount(DeleteAccountDto dto)
    {   
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value!;

        var result = await mediator.Send(new DeleteAccountCommand(userId, dto.Password, dto.GoogleIdToken));

        return Ok(result);
    }  
}