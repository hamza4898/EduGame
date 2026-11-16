using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Application.Auth.ConfirmRegister;

public class ConfirmRegisterCommandHandler(
    IIdentityService identityService,
    IIdentityRepository identityRepository,
    IAuthService authService
) : IRequestHandler<ConfirmEmailCommand, Result<ConfirmEmailResponseDto>>
{
    public async Task<Result<ConfirmEmailResponseDto>> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await identityService.FindByEmailAsync(request.Email);

        if (user == null) return Result.NotFound("Пользователь не найден.");

        var latestCodeEntity = await identityRepository.GetLatestResetCodeByUserId(user.Id);

        if (latestCodeEntity == null || latestCodeEntity.ExpiresAt < DateTime.UtcNow)
            return Result.Unauthorized("Код подтверждения протух или не существовал. Запросите код заново.");

        bool isCodeValid = BCrypt.Net.BCrypt.Verify(request.Code, latestCodeEntity.HashedCode);

        if (!isCodeValid) return Result.Unauthorized("Введен неверный код подтверждения!");

        user.EmailConfirmed = true;
        var updateResult = await identityService.Update(user);
    
        if (!updateResult.Succeeded) return Result.Error("Не удалось обновить статус активации пользователя.");

        await identityRepository.DeactivateUnusedResetCodes(user.Id);

        var tokensResult = await authService.GenerateSession(user);

        if (tokensResult == null)
        {
            await identityService.Delete(user);
            return Result.Conflict("Ошибка токенов");
        }

        return Result.Success(new ConfirmEmailResponseDto(user.Id, tokensResult.AccessToken, tokensResult.RefreshToken));
    }
}