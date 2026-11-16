using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Application.Auth.ForgotPassword;

public class ForgotPasswordCommandHandler(
    IIdentityService identityService,
    IAuthService authService,
    IEmailService emailService
) : IRequestHandler<ForgotPasswordCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await identityService.FindByEmailAsync(request.Email);

        if (user == null) return Result.Success();

        var codeResult = await authService.GeneratePasswordResetCode(user);

        await emailService.SendCodeToEmail(request.ClientRoute, codeResult.SessionId, request.Email, codeResult.Code);

        return Result.Success(codeResult.SessionId);
    }
}