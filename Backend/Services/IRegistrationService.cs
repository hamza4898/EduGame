using EduGame.DTOs;
using EduGame.Entities;

namespace EduGame.Services
{
    public interface IRegistrationService<T, D>
    {
        Task<T> CreateUser(D userDTO);

        Task<T> GetUserByExternalId(Guid externalId);
    }
}