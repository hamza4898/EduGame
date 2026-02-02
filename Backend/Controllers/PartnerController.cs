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
    public class PartnerController : RegisterController<Partner, PartnerDTO>
    {
       public PartnerController(IBaseUserService<Partner, PartnerDTO> service, IMapper mapper) : base(service, mapper) {}

       [ActionName("RegisterPartner")]
       public override async Task<IActionResult> RegisterUser(PartnerDTO partnerDTO) => await base.RegisterUser(partnerDTO);

       [HttpGet("{externalId}", Name = "GetPartner")]
       [ActionName("GetPartner")]
       public override async Task<IActionResult> GetUser(Guid externalId) => await base.GetUser(externalId);
    }
}