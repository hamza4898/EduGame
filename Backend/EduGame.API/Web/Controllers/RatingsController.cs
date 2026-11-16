using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using EduGame.Application.Courses.GetTeacherRatings;
using EduGame.Application.Courses.GetStudentsRating;
using EduGame.Application.Communities.GetCommunityRatings;
using EduGame.Application.Courses.GetCourseRatings;
using EduGame.Application.Stats.GetSkillPathZones;
using EduGame.Application.Stats.GetFocusTree;

namespace EduGame.Web.Controllers;

[ApiController]
[Authorize]
[Route("api/ratings/[action]")]
public class RatingsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ActionName("GetTopTeachers")]
    public async Task<IActionResult> GetTopTeachers(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetTeacherRatingsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ActionName("GetTopStudents")]
    public async Task<IActionResult> GetTopStudents()
    {
        var result = await mediator.Send(new GetStudentRatingsQuery());

        return Ok(result);
    }

    [HttpGet]
    [ActionName("GetTopCommunities")]
    public async Task<IActionResult> GetTopCommunities()
    {
        var result = await mediator.Send(new GetCommunityRatingsQuery());

        return Ok(result);
    }

    [HttpGet]
    [ActionName("GetTopCourses")]
    public async Task<IActionResult> GetTopCourses()
    {
        var result = await mediator.Send(new GetCourseRatingsQuery());

        return Ok(result);
    }

    [HttpGet]
    [ActionName("GetUserSkillPaths")]
    public async Task<IActionResult> GetUserSkillPaths()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value!; 

        var result = await mediator.Send(new GetSkillPathZonesQuery(userId));

        return Ok(result);
    }

    [HttpGet]
    [ActionName("GetFocusTree")]
    public async Task<IActionResult> GetFocusTree([FromQuery] string pathCode)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value!; 

        var result = await mediator.Send(new GetFocusTreeQuery(userId, pathCode));
    
        return Ok(result);
    }
}