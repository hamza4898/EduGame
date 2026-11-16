using EduGame.Domain.Entities;
using EduGame.Domain.Enums;

namespace EduGame.Application.Common.Interfaces;

public interface IIdentityRepository
{
    Task<RefreshToken?> GetStoredRefreshTokenWithUser(string hashedToken);

    Task RevokeAllRefreshTokensForUser(string userId);

    Task<PasswordResetCode?> GetPasswordResetCodeWithUser(Guid sessionId);

    Task Save();

    Task AddRefreshToken(RefreshToken refreshToken);

    Task DeactivateUnusedResetCodes(string userId);

    Task AddPasswordResetCode(PasswordResetCode passwordResetCode);

    Task<PasswordResetCode?> GetLatestResetCodeByUserId(string userId);

    Task<UserRole?> GetUserRole(string userId);

    Task<string?> GetUserName(string userId);

    Task<Dictionary<string, string?>> GetUserNames(IReadOnlyCollection<string> userIds);

    Task<Dictionary<string, string?>> GetAvatarUrls(IReadOnlyCollection<string> userIds);

    Task<string?> GetEmail(string userId);

    Task<ApplicationUser?> GetMySecurityPassport(string userId);

    Task<List<string>> GetAllUserIds(CancellationToken cancellationToken);

    Task Add(ApplicationUser user);
}