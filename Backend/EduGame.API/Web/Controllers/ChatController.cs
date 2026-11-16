using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using EduGame.Application.Communities.CreateCommunity;
using EduGame.Application.Chat.SearchCommunities;
using EduGame.Application.Chat.GetCommunityMembers;
using EduGame.Application.Common.Contracts;
using EduGame.Application.Chat.GetDeveloperMessages;
using EduGame.Application.Chat.GetAiSupportHistory;
using EduGame.Application.Chat.SupportAiChat;
using EduGame.Application.Chat.GetMessages;
using EduGame.Application.Chat.UploadChatAttachment;
using EduGame.Application.Chat.GetChatAttachment;
using EduGame.Application.Communities.SendCommunityInvite;
using EduGame.Application.Communities.AcceptCommunityInvite;
using EduGame.Application.Chat.GetMyCommunities;

namespace EduGame.Web.Controllers;

[ApiController]
[Authorize]
[Route("api/chat/[action]")]
public class ChatController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ActionName("CreateCommunity")]
    public async Task<IActionResult> CreateCommunity([FromBody] CreateCommunityDto dto, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateCommunityCommand(dto, CurrentUserId(), User.Identity?.Name ?? string.Empty), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ActionName("SearchCommunities")]
    public async Task<IActionResult> SearchCommunities([FromQuery] SearchCommunitiesDto dto, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new SearchCommunitiesQuery(dto.Query, dto.Offset, dto.Limit), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ActionName("GetCommunityMembers")]
    public async Task<IActionResult> GetCommunityMembers([FromQuery] GetCommunityMembersDto dto, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCommunityMembersQuery(dto.CommunityId, CurrentUserId(), dto.Offset, dto.Limit), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ActionName("AskAi")]
    [ProducesResponseType(typeof(AiSupportMessageResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AskAi([FromBody] UserPromptDto dto, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAiSupportQuery(CurrentUserId(), dto.UserPrompt), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ActionName("GetAiSupportHistory")]
    [ProducesResponseType(typeof(List<AiSupportMessageResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAiSupportHistory(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAiSupportHistoryQuery(CurrentUserId()), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ActionName("GetMyCommunities")]
    public async Task<IActionResult> GetMyCommunities(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetMyCommunitiesQuery(CurrentUserId()), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ActionName("GetMessages")]
    public async Task<IActionResult> GetMessages(
        [FromQuery] Guid communityId,
        [FromQuery] int offset,
        [FromQuery] int limit,
        [FromQuery] DateTime? since,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetMessagesQuery(communityId, CurrentUserId(), offset, limit, since), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ActionName("GetDeveloperMessages")]
    public async Task<IActionResult> GetDeveloperMessages([FromQuery] int offset, [FromQuery] int limit, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetDeveloperMessagesQuery(CurrentUserId(), offset, limit), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ActionName("SearchMessages")]
    [ProducesResponseType(typeof(List<ChatHistoryMessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SearchMessages(
        [FromQuery] Guid communityId,
        [FromQuery] string query,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new SearchMessagesQuery(communityId, CurrentUserId(), query), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [RequestSizeLimit(10 * 1024 * 1024 + 64 * 1024)]
    [ActionName("UploadAttachment")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadAttachment(
        [FromQuery] Guid communityId,
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UploadChatAttachmentCommand(communityId, CurrentUserId(), file),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("{communityId:guid}/{fileName}")]
    [ActionName("GetAttachment")]
    public async Task<IActionResult> GetAttachment(
        Guid communityId,
        string fileName,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetChatAttachmentQuery(communityId, fileName, CurrentUserId()),
            cancellationToken);
        if (!result.IsSuccess)
            return Ok(result);
        return PhysicalFile(result.Value.FilePath, result.Value.ContentType, enableRangeProcessing: true);
    }

    [HttpPost]
    [ActionName("InviteFriendToCommunity")]
    public async Task<IActionResult> InviteFriendToCommunity(
        [FromBody] InviteFriendDto dto,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new SendCommunityInviteCommand(CurrentUserId(), dto.ReceiverId, dto.CommunityId, dto.CommunityName),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ActionName("AcceptCommunityInvite")]
    public async Task<IActionResult> AcceptCommunityInvite(
        [FromBody] AcceptCommunityInviteDto dto,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new AcceptCommunityInviteCommand(CurrentUserId(), dto.CommunityId, dto.NotificationId),
            cancellationToken);
        return Ok(result);
    }

    private string CurrentUserId() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? User.FindFirst("sub")?.Value
        ?? string.Empty;
}