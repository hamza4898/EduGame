using Microsoft.AspNetCore.Mvc;
using EduGame.Services;
using AutoMapper;
using EduGame.DTOs;

namespace EduGame.Controllers
{
    [ApiController]
    public abstract class RegisterController<T, D> : ControllerBase
        where T: class
        where D: BaseRegistrationDto
    {
        protected readonly IRegistrationService<T, D> _registrationService;
        protected readonly IMapper _mapper;

        public RegisterController(IRegistrationService<T, D> registrationService, IMapper mapper)
        {
            _registrationService = registrationService;
            _mapper = mapper;
        }

        [HttpPost]
        [ActionName("RegisterUser")]
        public virtual async Task<IActionResult> RegisterUser(D userDto)
        {
            var result = await _registrationService.CreateUser(userDto);

            return CreatedAtRoute($"Get{typeof(T).Name}", new { externalId = ((dynamic)result.ValueOrDefault)?.ExternalId }, result);
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