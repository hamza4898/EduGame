using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Auth.ChangePassword;

public class ChangePasswordCommandHandler(
    IIdentityRepository identityRepository,
    IAuthService authService,
    IIdentityService identityService
) : IRequestHandler<ChangePasswordCommand, Result<LoginResponseDto>>
{
    public async Task<Result<LoginResponseDto>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var storedCode = await identityRepository.GetPasswordResetCodeWithUser(request.SessionId);

        if (storedCode == null || !storedCode.IsActive) return Result.Forbidden("Код недействителен");

        if (!storedCode.IsVerified)
            return Result.Forbidden("Действие не разрешено. Сначала подтвердите код");

        if (!storedCode.VerifiedAt.HasValue || storedCode.VerifiedAt.Value.AddMinutes(5) < DateTime.UtcNow)
        {
            storedCode.IsVerified = false;
            await identityRepository.Save();
            return Result.Forbidden("Время ожидания пароля истекло. Проверьте код еще раз");
        }

        var user = storedCode.User;

        var passwordResetResult = await identityService.ResetPassword(user, request.NewPassword);
        if (!passwordResetResult.Succeeded)
        {
            var errors = string.Join(" ", passwordResetResult.Errors.Select(error => error.Description));
            return Result.Error(errors);
        }

        await identityRepository.RevokeAllRefreshTokensForUser(user.Id);

        storedCode.IsUsed = true;

        await identityRepository.Save();

        var isTwoFactorEnabled = user.TwoFactorEnabled;

        var tokens = await authService.GenerateSession(user);

        return Result.Success(new LoginResponseDto(tokens.AccessToken, tokens.RefreshToken, isTwoFactorEnabled));
    }
}