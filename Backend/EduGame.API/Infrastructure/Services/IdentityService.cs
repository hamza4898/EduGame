using Microsoft.AspNetCore.Identity;
using EduGame.Domain.Entities;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Infrastructure.Services;

public class IdentityService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager
) : IIdentityService
{
    public async Task<ApplicationUser?> FindByEmailAsync(string email) =>
        await userManager.FindByEmailAsync(email);

    public async Task<ApplicationUser?> FindById(string userId) =>
        await userManager.FindByIdAsync(userId);

    public async Task<SignInResult> CheckPasswordSignInAsync(ApplicationUser user, string password, bool lockoutOnFailure) =>
        await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure);

    public async Task<IdentityResult> Update(ApplicationUser user) =>
        await userManager.UpdateAsync(user);

    public async Task<IdentityResult> Delete(ApplicationUser user) =>
        await userManager.DeleteAsync(user);

    public async Task<IdentityResult> CreateAsync(ApplicationUser user, string password) =>
        await userManager.CreateAsync(user, password);

    public async Task<IdentityResult> AddLogin(ApplicationUser user, UserLoginInfo loginInfo) =>
        await userManager.AddLoginAsync(user, loginInfo);

    public async Task<IdentityResult> CreateAsync(ApplicationUser user) =>
        await userManager.CreateAsync(user);

    public async Task<IList<UserLoginInfo>> GetLoginsAsync(ApplicationUser user) =>
        await userManager.GetLoginsAsync(user);

    public async Task<IdentityResult> ResetAuthenticatorKey(ApplicationUser user) =>
        await userManager.ResetAuthenticatorKeyAsync(user);

    public async Task<string?> GetAuthenticatorKey(ApplicationUser user) =>
        await userManager.GetAuthenticatorKeyAsync(user);

    public async Task<bool> VerifyTwoFactorToken(ApplicationUser user, string code) =>
        await userManager.VerifyTwoFactorTokenAsync(user, userManager.Options.Tokens.AuthenticatorTokenProvider, code);

    public async Task<IdentityResult> SetTwoFactorEnabled(ApplicationUser user, bool setState) =>
        await userManager.SetTwoFactorEnabledAsync(user, setState);

    public async Task<IdentityResult> ResetPassword(ApplicationUser user, string newPassword)
    {
        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        return await userManager.ResetPasswordAsync(user, resetToken, newPassword);
    }

    public async Task<IList<string>> GetRolesAsync(ApplicationUser user) =>
        await userManager.GetRolesAsync(user);

    public async Task AddToRole(ApplicationUser user, string role) =>
        await userManager.AddToRoleAsync(user, role);

    public async Task SetEmail(ApplicationUser user, string newEmail) =>
        await userManager.SetEmailAsync(user, newEmail);

    public async Task<bool> CheckPassword(ApplicationUser user, string currentPassword) =>
        await userManager.CheckPasswordAsync(user, currentPassword);

    public async Task<bool> HasPassword(ApplicationUser user) =>
        await userManager.HasPasswordAsync(user);

    public async Task<IdentityResult> RemoveLogin(ApplicationUser user, string loginProvider, string providerKey) =>
        await userManager.RemoveLoginAsync(user, loginProvider, providerKey);

    public async Task<ApplicationUser?> FindByLogin(string loginProvider, string providerKey) => 
        await userManager.FindByLoginAsync(loginProvider, providerKey);
}