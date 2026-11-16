using System.Security.Claims;
using EduGame.Application.Common.Contracts;
using EduGame.Application.Developer.BroadcastDeveloperNotification;
using EduGame.Application.Developer.CreateDeveloperUpdate;
using EduGame.Application.Developer.GetDeveloperAdminAccess;
using EduGame.Application.Developer.GetDeveloperUpdates;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduGame.Web.Controllers;

[ApiController]
[Authorize]
[Route("api/developer/[action]")]
public sealed class DeveloperController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ActionName("GetAdminAccess")]
    public async Task<IActionResult> GetAdminAccess(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value!;

        var result = await mediator.Send(
            new GetDeveloperAdminAccessQuery(userId),
            cancellationToken);
        
        return Ok(result);
    }

    [HttpGet]
    [ActionName("GetUpdates")]
    [ProducesResponseType(typeof(IReadOnlyList<DeveloperUpdateResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUpdates(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetDeveloperUpdatesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    [ActionName("CreateUpdate")]
    [ProducesResponseType(typeof(DeveloperUpdateResponseDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateUpdate(
        [FromBody] CreateDeveloperUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateDeveloperUpdateCommand(request), cancellationToken);
        if (result.IsSuccess)
            return Created("/api/developer/GetUpdates", result.Value);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    [ActionName("BroadcastNotification")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> BroadcastNotification(
        [FromBody] BroadcastDeveloperNotificationRequest request,
        CancellationToken cancellationToken)
    {
        var senderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? string.Empty;
        var result = await mediator.Send(
            new BroadcastDeveloperNotificationCommand(senderId, request.Message),
            cancellationToken);
        return Ok(result);
    }
}