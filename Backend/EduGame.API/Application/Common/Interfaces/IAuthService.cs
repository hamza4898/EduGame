using EduGame.Application.Common.Contracts;
using EduGame.Domain.Entities;

namespace EduGame.Application.Common.Interfaces;

public interface IAuthService
{
    Task<TokenResponseDto> GenerateSession(ApplicationUser user);

    Task<ResetCodeResultDto> GeneratePasswordResetCode(ApplicationUser user);
}