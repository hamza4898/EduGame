using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Application.Users.GetMySecurityPassport;

public class GetMySecurityPassportQueryHandler(
    IIdentityService identityService
) : IRequestHandler<GetMySecurityPassportQuery, Result<MySecurityPassportResponseDto>>
{
    public async Task<Result<MySecurityPassportResponseDto>> Handle(GetMySecurityPassportQuery request, CancellationToken cancellationToken)
    {
        var user = await identityService.FindById(request.MyId);
        if (user == null) return Result.NotFound("Пользователь не найден");

        var logins = await identityService.GetLoginsAsync(user);
        var hasGoogle = logins.Any(l => l.LoginProvider == "Google");

        var passport = new MySecurityPassportResponseDto(
            Id: user.Id,
            RegisteredAt: user.RegisteredAt,
            UserRole: user.UserRole,
            Email: user.Email ?? "",
            EmailConfirmed: user.EmailConfirmed,
            TwoFactorEnabled: user.TwoFactorEnabled,
            HasPassword: !string.IsNullOrEmpty(user.PasswordHash),
            HasGoogleLogin: hasGoogle,
            GoogleEmail: hasGoogle ? user.Email : null
        );

        return Result.Success(passport);
    }
}