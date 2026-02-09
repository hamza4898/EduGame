using EduGame.DTOs;

namespace EduGame.Services
{
    public interface IRegistrationService<T, D>
    {
        Task<T> CreateUser(D userDTO);

        Task<T> GetUserByExternalId(Guid externalId);
    }
}