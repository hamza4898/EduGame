using EduGame.Domain.Entities;
using EduGame.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using EduGame.Application.Common.Interfaces;
using EduGame.Domain.Enums;

namespace EduGame.Infrastructure.Repositories;

public class IdentityRepository(EduGameDbContext context) : IIdentityRepository
{
    public async Task AddRefreshToken(RefreshToken refreshToken) => 
        await context.RefreshTokens.AddAsync(refreshToken);

    public async Task<RefreshToken?> GetStoredRefreshTokenWithUser(string? hashedToken) => 
        await context.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(rt => rt.HashedToken == hashedToken);

    public async Task RevokeAllRefreshTokensForUser(string userId) =>
        await context.RefreshTokens
            .Where(t => t.UserId == userId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.IsRevoked, true)
                .SetProperty(t => t.RevokedAt, DateTime.UtcNow));

    public async Task<PasswordResetCode?> GetPasswordResetCodeWithUser(Guid sessionId) =>
        await context.PasswordResetCodes
            .Include(u => u.User)
            .FirstOrDefaultAsync(x => x.ExternalId == sessionId);    

    public async Task DeactivateUnusedResetCodes(string userId) =>
        await context.PasswordResetCodes
            .Where(u => u.UserId == userId && !u.IsUsed)
            .ExecuteUpdateAsync(c => c.SetProperty(t => t.IsUsed, true));

    public async Task AddPasswordResetCode(PasswordResetCode passwordResetCode) => 
        await context.PasswordResetCodes.AddAsync(passwordResetCode);

    public async Task<PasswordResetCode?> GetLatestResetCodeByUserId(string userId) =>
        await context.PasswordResetCodes
            .Where(x => x.UserId == userId 
                    && !x.IsUsed
                    && x.Attempts <= 5
                    && x.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync();

    public async Task<string?> GetUserName(string userId) =>
        await context.Users
            .Where(x => x.Id == userId)
            .Select(x => x.UserName)
            .FirstOrDefaultAsync();

    public async Task<Dictionary<string, string?>> GetUserNames(IReadOnlyCollection<string> userIds) =>
        await context.Users
            .Where(user => userIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, user => user.UserName);

    public async Task<Dictionary<string, string?>> GetAvatarUrls(IReadOnlyCollection<string> userIds) =>
        await context.BaseUsers
            .Where(user => userIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, user => user.AvatarUrl);

    public async Task<string?> GetEmail(string userId) =>
        await context.Users
            .Where(x => x.Id == userId)
            .Select(x => x.Email)
            .FirstOrDefaultAsync();

    public async Task<UserRole?> GetUserRole(string userId) =>
        await context.Users
            .AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => x.UserRole)
            .FirstOrDefaultAsync();

    public async Task<ApplicationUser?> GetMySecurityPassport(string userId) =>
        await context.Users
            .AsNoTracking()
            .Where(x => x.Id == userId)
            .FirstOrDefaultAsync();

    public async Task<List<string>> GetAllUserIds(CancellationToken cancellationToken) =>
        await context.Users
            .AsNoTracking()
            .Select(user => user.Id)
            .ToListAsync(cancellationToken);

    public async Task Save() => await context.SaveChangesAsync();

    public async Task Add(ApplicationUser user) =>
        await context.AddAsync(user);
}