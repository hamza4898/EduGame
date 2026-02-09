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
            var userEntity = await _registrationService.CreateUser(userDto);

            var responceDto = _mapper.Map<D>(userEntity);

            return CreatedAtRoute($"Get{typeof(T).Name}", new { externalId = ((dynamic)userEntity).ExternalId }, responceDto);
        }

        [HttpGet("{externalId}")]
        [ActionName("GetUser")]
        public virtual async Task<IActionResult> GetUser(Guid externalId)
        {
            var user = await _registrationService.GetUserByExternalId(externalId);

            if (user == null) return NotFound();

            var responce = _mapper.Map<D>(user);

            return Ok(responce);
        }
    }
}