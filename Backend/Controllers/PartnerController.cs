using Microsoft.AspNetCore.Mvc;
using EduGame.Services;
using EduGame.DTOs;
using EduGame.Entities;
using System.Runtime.Versioning;

namespace EduGame.Controllers
{
    [ApiController]
    [Route("api/partner/[action]")]
    public class PartnerController : ControllerBase
    {
        private readonly IBaseUserService<Partner, PartnerDTO> _service;

        public PartnerController(IBaseUserService<Partner, PartnerDTO> service)
        {
            _service = service;
        }  

        [HttpPost]
        public async Task<IActionResult> PartnerRegister(PartnerDTO partnerDTO)
        {
            await _service.CreateUser(partnerDTO);
            return Ok("Партнер успешно добавлен в базу данных!");
        } 
    }
}