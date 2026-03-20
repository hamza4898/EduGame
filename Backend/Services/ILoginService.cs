using Ardalis.Result;
using EduGame.DTOs;

namespace EduGame.Services
{
    public interface ILoginService
    {
        Task<Result<string>> LoginUser(LoginDto loginDto);
    }
}