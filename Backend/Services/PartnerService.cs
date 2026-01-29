using EduGame.Entities;
using BCrypt.Net;
using EduGame.DTOs;
using System.Threading.Tasks;

namespace EduGame.Services
{
    public class PartnerService : IPartnerService
    {
        private readonly EFCoreDbContext _eduGameDbContext;

        public PartnerService(EFCoreDbContext eduGameDbContext)
        {
            _eduGameDbContext = eduGameDbContext;
        }

        public async Task CreatePartner(PartnerDTO partnerDTO)
        {
            var partner = new Partner
            {
                Company = partnerDTO.Company,
                FirstName = partnerDTO.FirstName,
                LastName = partnerDTO.LastName,
                Phone = partnerDTO.Phone,
                TypeOfCooperation = partnerDTO.TypeOfCooperation,
                Email = partnerDTO.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(partnerDTO.Password),
                Motivation = partnerDTO.Motivation
            };

            _eduGameDbContext.Partners.Add(partner);

            await _eduGameDbContext.SaveChangesAsync();
        }
    }
}