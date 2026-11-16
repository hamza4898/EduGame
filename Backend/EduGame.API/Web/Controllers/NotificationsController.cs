using System.Security.Claims;
using EduGame.Application.Courses.GetMyNotifications;
using EduGame.Application.Notifications.MarkNotificationRead;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduGame.Web.Controllers;

[ApiController]
[Authorize]
[Route("api/notifications/[action]")]
public sealed class NotificationsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMyNotifications(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetMyNotificationsQuery(CurrentUserId()), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> MarkNotificationRead(int notificationId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new MarkNotificationReadCommand(CurrentUserId(), notificationId),
            cancellationToken);
        return Ok(result);
    }

    private string CurrentUserId() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value ?? string.Empty;
}