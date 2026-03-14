using Microsoft.AspNetCore.Mvc;
using EduGame.Services;
using EduGame.DTOs;
using Ardalis.Result;

namespace EduGame.Controllers
{
    [ApiController]
    public abstract class RegisterController<T, D> : ControllerBase
        where T: class
        where D: BaseRegistrationDto
    {
        protected readonly IRegistrationService<T, D> _registrationService;

        public RegisterController(IRegistrationService<T, D> registrationService) => _registrationService = registrationService;

        [HttpPost]
        [ActionName("RegisterUser")]
        public virtual async Task<IActionResult> RegisterUser(D userDto)
        {
            var result = await _registrationService.CreateUser(userDto);

            return CreatedAtRoute($"Get{typeof(T).Name}", new { externalId = ((dynamic)result.Value)?.ExternalId }, result);
        }

        [HttpGet("{externalId}")]
        [ActionName("GetUser")]
        public virtual async Task<IActionResult> GetUser(Guid externalId)
        {
            var result = await _registrationService.GetUserByExternalId(externalId);

            return Ok(result);
        }
    }
}