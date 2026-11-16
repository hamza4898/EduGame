using MediatR;
using Microsoft.AspNetCore.Mvc;
using EduGame.Web.Filters;
using EduGame.Domain.Entities;
using EduGame.Application.Auth.Registration;

namespace EduGame.Web.Controllers;

[ApiController]
[Route("api/register/[action]")]
public abstract class RegisterController<T, D>(IMediator mediator) : ControllerBase
    where T : BaseUser
    where D : BaseRegistrationDto
{
    [HttpPost]
    [ActionName("RegisterUser")]
    [TypeFilter(typeof(ResultValidationFilter))]
    public virtual async Task<IActionResult> RegisterUser([FromBody] D userDto)
    {
        var command = new RegistrationCommand<T, D>(userDto);

        var result = await mediator.Send(command);

        return Ok(result);
    }
}