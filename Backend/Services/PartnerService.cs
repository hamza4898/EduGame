using EduGame.Entities;
using BCrypt.Net;
using EduGame.DTOs;
using System.Threading.Tasks;
using AutoMapper;

namespace EduGame.Services
{
    public class PartnerService : IPartnerService
    {
        private readonly EFCoreDbContext _eduGameDbContext;
        private readonly IMapper _mapper;

        public PartnerService(EFCoreDbContext eduGameDbContext, IMapper mapper)
        {
            _eduGameDbContext = eduGameDbContext;
            _mapper = mapper;
        }

        public async Task CreatePartner(PartnerDTO partnerDTO)
        {
            var partner = _mapper.Map<Partner>(partnerDTO);

            partner.PasswordHash = BCrypt.Net.BCrypt.HashPassword(partnerDTO.Password);

            _eduGameDbContext.Partners.Add(partner);
            await _eduGameDbContext.SaveChangesAsync();
        }
    }
}