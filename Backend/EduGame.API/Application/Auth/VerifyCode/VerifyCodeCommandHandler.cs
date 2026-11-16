using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Application.Auth.VerifyCode;

public class VerifyCodeCommandHandler(
    IIdentityRepository identityRepository
) : IRequestHandler<VerifyCodeCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(VerifyCodeCommand request, CancellationToken cancellationToken)
    {
        var storedCode = await identityRepository.GetPasswordResetCodeWithUser(request.SessionId);

        if (storedCode == null || !storedCode.IsActive) return Result.Forbidden("Код недействителен");

        var isCodeValid = BCrypt.Net.BCrypt.Verify(request.Code, storedCode.HashedCode);

        if (!isCodeValid)
        {
            storedCode.Attempts++;
            if (storedCode.Attempts > 5)
            {
                await identityRepository.Save();
                return Result.Forbidden("Неверный код подтверждения");    
            }

            await identityRepository.Save();
            return Result.Forbidden("Неверный код подтверждения"); 
        }

        storedCode.IsVerified = true;
        storedCode.VerifiedAt = DateTime.UtcNow;
        await identityRepository.Save();

        return Result.Success();
    }
}