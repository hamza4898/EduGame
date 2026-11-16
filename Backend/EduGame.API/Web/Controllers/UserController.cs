using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using EduGame.Application.Users.GetMyBusinessPassport;
using EduGame.Application.Users.GetMySecurityPassport;
using EduGame.Application.Auth.CompleteProfile;
using EduGame.Application.Users.UpdateMyProfile;

namespace EduGame.Web.Controllers;

[ApiController]
[Route("api/user/[action]")]
[Authorize]
public class UserController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ActionName("GetMyBusinessPassport")]
    public async Task<IActionResult> GetMyBusinessPassport(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetMyBusinessPassportQuery(CurrentUserId(), User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ActionName("GetBusinessPassport")]
    public async Task<IActionResult> GetBusinessPassport(
        [FromQuery] string? id,
        [FromQuery] string? nick,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetMyBusinessPassportQuery(id!, nick!), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [RequestSizeLimit(5 * 1024 * 1024 + 128 * 1024)]
    [ActionName("UpdateMyProfile")]
    public async Task<IActionResult> UpdateMyProfile(
        [FromForm] UpdateMyProfileRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        var result = await mediator.Send(new UpdateMyProfileCommand(userId, request), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ActionName("GetMySecurityPassport")]
    public async Task<IActionResult> GetMySecurityPassport(CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();

        var result = await mediator.Send(new GetMySecurityPassportQuery(userId), cancellationToken);

        return Ok(result);  
    }

    [HttpPost]
    [ActionName("CompleteProfile")]
    public async Task<IActionResult> CompleteProfile(
        [FromBody] CompleteProfileDto dto,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();

        var result = await mediator.Send(new CompleteProfileCommand(userId, dto), cancellationToken);

        return Ok(result);
    }

    private string CurrentUserId() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? User.FindFirst("sub")?.Value
        ?? string.Empty;
}