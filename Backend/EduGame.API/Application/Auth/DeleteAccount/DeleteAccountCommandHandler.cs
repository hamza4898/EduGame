using MediatR;
using Ardalis.Result;
using Google.Apis.Auth;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Application.Auth.DeleteAccount;

public class DeleteAccountCommandHandler(
    IConfiguration configuration,
    IIdentityService identityService
) : IRequestHandler<DeleteAccountCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var user = await identityService.FindById(request.UserId);

        if (user == null) return Result.NotFound("Пользователь не найден");

        var hasPassword = await identityService.HasPassword(user);

        if (hasPassword)
        {
            if (string.IsNullOrEmpty(request.Password)) return Result.Error("Необходимо ввести пароль");

            var isPasswordValid = await identityService.CheckPassword(user, request.Password);

            if (!isPasswordValid) return Result.Error("Неверный пароль");
        }
        else
        {
            var googleLogin = (await identityService.GetLoginsAsync(user))
                .FirstOrDefault(login => login.LoginProvider == "Google");
            if (googleLogin == null)
                return Result.Error("Для удаления аккаунта требуется настроенный способ повторной аутентификации.");

            if (string.IsNullOrEmpty(request.GoogleIdToken))
                return Result.Error("Требуется повторный вход через Google");

            GoogleJsonWebSignature.Payload payload;
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { configuration["Authentication:Google:ClientId"] }
                };
                payload = await GoogleJsonWebSignature.ValidateAsync(request.GoogleIdToken, settings);
            }
            catch (InvalidJwtException)
            {
                return Result.Error("Невалидный Google токен");
            }

            if (!string.Equals(payload.Subject, googleLogin.ProviderKey, StringComparison.Ordinal))
                return Result.Error("Google-аккаунт не совпадает с привязанным к профилю.");
        }

        var userLogins = await identityService.GetLoginsAsync(user);
        foreach (var login in userLogins)
        {
            await identityService.RemoveLogin(user, login.LoginProvider, login.ProviderKey);
        }

        var deleteResult = await identityService.Delete(user);

        if (!deleteResult.Succeeded) return Result.Error("Не удалось удалить аккаунт из системы");

        return Result.Success();
    }
}