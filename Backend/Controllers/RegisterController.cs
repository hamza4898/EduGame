using Microsoft.AspNetCore.Mvc;
using EduGame.Services;
using AutoMapper;
using EduGame.DTOs;

namespace EduGame.Controllers
{
    [ApiController]
    public abstract class RegisterController<T, D> : ControllerBase
        where T: class
        where D: BaseRegistrationDTO
    {
        protected readonly IBaseUserService<T, D> _service;
        protected readonly IMapper _mapper;

        public RegisterController(IBaseUserService<T, D> service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpPost]
        [ActionName("RegisterUser")]
        public virtual async Task<IActionResult> RegisterUser(D userDTO)
        {
            var userEntity = await _service.CreateUser(userDTO);

            var responceDTO = _mapper.Map<D>(userEntity);

            return CreatedAtRoute($"Get{typeof(T).Name}", new { externalId = ((dynamic)userEntity).ExternalId }, responceDTO);
        }

        [HttpGet("{externalId}")]
        [ActionName("GetUser")]
        public virtual async Task<IActionResult> GetUser(Guid externalId)
        {
            var user = await _service.GetUserByExternalId(externalId);

            if (user == null) return NotFound();

            var responce = _mapper.Map<D>(user);

            return Ok(responce);
        }
    }
}