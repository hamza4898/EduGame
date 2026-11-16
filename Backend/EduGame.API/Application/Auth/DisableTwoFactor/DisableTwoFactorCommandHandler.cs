using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Application.Auth.DisableTwoFactor;

public class DisableTwoFactorCommandHandler(IIdentityService identityService) : IRequestHandler<DisableTwoFactorCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(DisableTwoFactorCommand request, CancellationToken cancellationToken)
    {
        var user = await identityService.FindById(request.UserId);

        if (user == null) return Result.NotFound("Пользователь с таким ID не существует");

        var isCodeValid = await identityService.VerifyTwoFactorToken(user, request.Code);

        if (!isCodeValid) return Result.Error("Введен неверный или устаревший код подтверждения");

        var disableResult = await identityService.SetTwoFactorEnabled(user, false);

        if (!disableResult.Succeeded) return Result.Error("Не удалось обновить статус безопасности аккаунта.");

        return Result.Success();       
    }
}