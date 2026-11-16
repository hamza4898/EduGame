using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Application.Auth.VerifyTwoFactor;

public class VerifyTwoFactorCommandHandler(IIdentityService identityService) : IRequestHandler<VerifyTwoFactorCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(VerifyTwoFactorCommand request, CancellationToken cancellationToken)
    {
        var user = await identityService.FindById(request.UserId);

        if (user == null) return Result.NotFound("Пользователь не найден");

        var isValid = await identityService.VerifyTwoFactorToken(user, request.Code);

        if (!isValid) return Result.Invalid(new ValidationError("Code", "Введен неверный или устаревший код подтверждения"));

        var enableResult = await identityService.SetTwoFactorEnabled(user, true);

        if (!enableResult.Succeeded) return Result.Error("Не удалось обновить статус безопасности аккаунта.");

        return Result.Success();
    }
}