using System.Runtime.CompilerServices;
using EduGame.DTOs;

namespace EduGame.Services
{
    public interface IBaseUserService<T, D>
    {
        Task<T> CreateUser(D userDTO);

        Task<T> GetUserById(int id);
    }
}