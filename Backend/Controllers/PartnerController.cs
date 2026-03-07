using Microsoft.AspNetCore.Mvc;
using EduGame.Services;
using EduGame.DTOs;
using EduGame.Entities;

namespace EduGame.Controllers
{
    [ApiController]
    [Route("api/partner/[action]")]
    public class PartnerController : RegisterController<Partner, PartnerDto>
    {
       public PartnerController(IRegistrationService<Partner, PartnerDto> registrationService) : base(registrationService) {}

       [ActionName("RegisterPartner")]
       public override async Task<IActionResult> RegisterUser(PartnerDto partnerDto) => await base.RegisterUser(partnerDto);

       [HttpGet("{externalId}", Name = "GetPartner")]
       [ActionName("GetPartner")]
       public override async Task<IActionResult> GetUser(Guid externalId) => await base.GetUser(externalId);
    }
}