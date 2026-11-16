using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Application.Auth.UnlinkGoogleAccount;

public class UnlinkGoogleAccountCommandHandler(IIdentityService identityService) : IRequestHandler<UnlinkGoogleAccountCommand, Result>
{
    public async Task<Result> Handle(UnlinkGoogleAccountCommand request, CancellationToken cancellationToken)
    {
        var currentUser = await identityService.FindById(request.UserId);

        if (currentUser == null) return Result.NotFound("Пользователь не найден");

        var currentLogins = await identityService.GetLoginsAsync(currentUser);
        var googleLogin = currentLogins.FirstOrDefault(l => l.LoginProvider == "Google");
        
        if (googleLogin == null) return Result.Error("Google-аккаунт не привязан к вашему профилю");

        var hasPassword = await identityService.HasPassword(currentUser);

        if (!hasPassword && currentLogins.Count == 1) return Result.Error("Нельзя отвязать Google, так как это единственный способ входа в ваш аккаунт. Сначала установите пароль в профиле.");

        var result = await identityService.RemoveLogin(
            currentUser, 
            googleLogin.LoginProvider, 
            googleLogin.ProviderKey
        );

        if (!result.Succeeded) return Result.Error("Не удалось отвязать Google-аккаунт");

        return Result.Success();
    }
}