using EduGame.Application.Common.Contracts;
using EduGame.Application.Users.UpdateMyProfile;

namespace EduGame.Application.Common.Interfaces;

public interface IUserProfileService
{
    Task<MyBusinessPassportResponseDto?> GetBusinessPassportAsync(
        string? userId,
        string? userName,
        CancellationToken cancellationToken);

    Task UpdateMyProfileAsync(
        string userId,
        UpdateMyProfileRequestDto request,
        CancellationToken cancellationToken);
}