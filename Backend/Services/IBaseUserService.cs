using EduGame.DTOs;

namespace EduGame.Services
{
    public interface IBaseUserService<T, D>
    {
        Task CreateUser(D userDTO);
    }
}