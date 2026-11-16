using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Application.Auth.EnableTwoFactor;

public class EnableTwoFactorCommandHandler(
    IIdentityService identityService, 
    IQrCodeService qrCodeService)
    : IRequestHandler<EnableTwoFactorCommand, Result<EnableTwoFactorResponseDto>>
{
    public async Task<Result<EnableTwoFactorResponseDto>> Handle(EnableTwoFactorCommand request, CancellationToken cancellationToken)
    {
        var user = await identityService.FindById(request.UserId);
        if (user == null) 
            return Result.NotFound("Пользователь с таким ID не найден");

        var resetResult = await identityService.ResetAuthenticatorKey(user);
        if (!resetResult.Succeeded) 
            return Result.Error("Критическая ошибка генерации ключа защиты");

        var unformattedKey = await identityService.GetAuthenticatorKey(user);
        if (string.IsNullOrEmpty(unformattedKey)) 
            return Result.Error("Не удалось извлечь ключ синхронизации.");

        var username = user.UserName ?? user.Email ?? "User";
        var qrCodeUrl = qrCodeService.CreateTotpUrl("EduGame", username, unformattedKey);
        var base64Image = qrCodeService.GenerateBase64Png(qrCodeUrl);

        return Result.Success(new EnableTwoFactorResponseDto(unformattedKey, base64Image));      
    }
}