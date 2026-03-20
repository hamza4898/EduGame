using Ardalis.Result;
using EduGame.Entities;

namespace EduGame.Services
{
    public interface IJwtService
    {
        Result<string> GenerateToken(ApplicationUser user);
    }
}