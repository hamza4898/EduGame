using EduGame.DTOs;
using EduGame.Entities;
using FluentResults;

namespace EduGame.Services
{
    public interface IRegistrationService<T, D>
    {
        Task<Result<T>> CreateUser(D userDTO);

        Task<Result<T>> GetUserByExternalId(Guid externalId);
    }
}