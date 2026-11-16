using EduGame.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace EduGame.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<ApplicationUser?> FindByEmailAsync(string email);

    Task<ApplicationUser?> FindById(string userId);

    Task<SignInResult> CheckPasswordSignInAsync(ApplicationUser user, string password, bool lockoutOnFailure);

    Task<IdentityResult> Update(ApplicationUser user);

    Task<IdentityResult> Delete(ApplicationUser user);

    Task<IdentityResult> CreateAsync(ApplicationUser user, string password);

    Task<IdentityResult> CreateAsync(ApplicationUser user);

    Task<IdentityResult> AddLogin(ApplicationUser user, UserLoginInfo loginInfo);

    Task<IList<UserLoginInfo>> GetLoginsAsync(ApplicationUser user);

    Task<string?> GetAuthenticatorKey(ApplicationUser user);

    Task<IdentityResult> ResetAuthenticatorKey(ApplicationUser user);

    Task<bool> VerifyTwoFactorToken(ApplicationUser user, string code);

    Task<IdentityResult> SetTwoFactorEnabled(ApplicationUser user, bool setState);

    Task<IdentityResult> ResetPassword(ApplicationUser user, string newPassword);

    Task<IList<string>> GetRolesAsync(ApplicationUser user);

    Task AddToRole(ApplicationUser user, string role);

    Task SetEmail(ApplicationUser user, string newEmail);

    Task<bool> CheckPassword(ApplicationUser user, string currentPassword);

    Task<bool> HasPassword(ApplicationUser user);

    Task<IdentityResult> RemoveLogin(ApplicationUser user, string loginProvider, string providerKey);

    Task<ApplicationUser?> FindByLogin(string loginProvider, string providerKey);
}