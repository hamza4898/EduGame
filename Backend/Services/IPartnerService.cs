using EduGame.DTOs;

namespace EduGame.Services
{
    public interface IPartnerService
    {
        Task CreatePartner(PartnerDTO partnerDTO);
    }
}