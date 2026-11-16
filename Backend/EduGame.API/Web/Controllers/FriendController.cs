using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using EduGame.Application.Friends.SearchUsers;
using EduGame.Application.Friends.AcceptFriendRequest;
using EduGame.Application.Friends.BlockFriendRequest;
using EduGame.Application.Friends.DeclineFriendRequest;
using EduGame.Application.Friends.DeleteFriend;
using EduGame.Application.Friends.GetMyFriends;
using EduGame.Application.Friends.GetIncomingRequests;
using EduGame.Application.Friends.GetOutgoingRequests;
using EduGame.Application.Friends.SendFriendRequest;

namespace EduGame.Web.Controllers;

[ApiController]
[Authorize]
[Route("api/friend/[action]")]
public class FriendController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ActionName("GetMyFriends")]
    public async Task<IActionResult> GetMyFriends()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value!; 
    
        var result = await mediator.Send(new GetMyFriendsQuery(userId));

        return Ok(result);
    }

    [HttpGet]
    [ActionName("GetMyIncomingRequests")]
    public async Task<IActionResult> GetMyIncomingRequests()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value!; 

        var result = await mediator.Send(new GetIncomingRequestsQuery(userId));

        return Ok(result);
    }

    [HttpGet]
    [ActionName("GetMyOutgoingRequests")]
    public async Task<IActionResult> GetMyOutgoingRequests()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value!; 

        var result = await mediator.Send(new GetOutgoingRequestQuery(userId));

        return Ok(result);
    }

    [HttpPost]
    [ActionName("SendFriendRequest")]
    public async Task<IActionResult> SendFriendRequest([FromQuery] string receiverId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value!; 

        var result = await mediator.Send(new SendFriendRequestCommand(receiverId, userId));

        return Ok(result);
    }

    [HttpGet]
    [ActionName("SearchUsers")]
    public async Task<IActionResult> SearchUsers([FromQuery] string query, [FromQuery] int limit = 10)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value!;

        var result = await mediator.Send(new SearchUsersQuery(userId, query, limit));

        return Ok(result);
    }

    [HttpPut]
    [ActionName("AcceptFriendRequest")]
    public async Task<IActionResult> AcceptFriendRequest(int requestId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value!; 

        var result = await mediator.Send(new AcceptFriendRequestCommand(userId, requestId));

        return Ok(result);      
    }

    [HttpPut]
    [ActionName("BlockFriendRequest")]
    public async Task<IActionResult> BlockFriendRequest(int requestId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value!; 

        var result = await mediator.Send(new BlockFriendRequestCommand(userId, requestId));

        return Ok(result);
    }

    [HttpDelete]
    [ActionName("DeclineFriendRequest")]
    public async Task<IActionResult> DeclineFriendRequest(int requestId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value!; 

        var result = await mediator.Send(new DeclineFriendRequestCommand(userId, requestId));

        return Ok(result);     
    }

    [HttpDelete]
    [ActionName("DeleteMyFriend")]
    public async Task<IActionResult> DeleteMyFriend(int requestId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value!; 

        var result = await mediator.Send(new DeleteFriendCommand(userId, requestId));

        return Ok(result);    
    }  
}