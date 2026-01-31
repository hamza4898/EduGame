using Microsoft.AspNetCore.Mvc;
using EduGame.Services;
using EduGame.DTOs;
using EduGame.Entities;
using System.Runtime.Versioning;
using AutoMapper;

namespace EduGame.Controllers
{
    [ApiController]
    [Route("api/partner/[action]")]
    public class PartnerController : ControllerBase
    {
        private readonly IBaseUserService<Partner, PartnerDTO> _service;
        private readonly IMapper _mapper;

        public PartnerController(IBaseUserService<Partner, PartnerDTO> service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }  

        [HttpPost]
        public async Task<IActionResult> PartnerRegister(PartnerDTO partnerDTO)
        {
            var partner = await _service.CreateUser(partnerDTO);

            var responseDTO = _mapper.Map<PartnerDTO>(partner);

            return CreatedAtRoute("GetPartner", new { id = partner.Id }, responseDTO);
        } 

        [HttpGet("{id}", Name = "GetPartner")]
        public async Task<IActionResult> GetPartnerById(int id)
        {
            var partner = await _service.GetUserById(id);

            if (partner == null) return NotFound();

            var responce = _mapper.Map<PartnerDTO>(partner);

            return Ok(responce);
        }
    }
}