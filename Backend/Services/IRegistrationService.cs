using EduGame.DTOs;
using EduGame.Entities;
using Ardalis.Result;

namespace EduGame.Services
{
    public interface IRegistrationService<T, D>
    {
        Task<Result<T>> CreateUser(D userDTO);

        Task<Result<T>> GetUserByExternalId(Guid externalId);
    }
}