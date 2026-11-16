using MediatR;
using Ardalis.Result;
using Google.Apis.Auth;
using EduGame.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace EduGame.Application.Auth.LinkGoogleToCurrentAccount;

public class LinkGoogleAccountCommandHandler(
    IConfiguration configuration,
    IIdentityService identityService
) : IRequestHandler<LinkGoogleAccountCommand, Result>
{
    public async Task<Result> Handle(LinkGoogleAccountCommand request, CancellationToken cancellationToken)
    {
        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = new[] { configuration["Authentication:Google:ClientId"] }
        };

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
        }
        catch (InvalidJwtException)
        {
            return Result.Error("Невалидный Google токен");
        }

        var userWithThisGoogle = await identityService.FindByLogin("Google", payload.Subject);
        
        if (userWithThisGoogle != null) return Result.Conflict("Этот Google-аккаунт уже привязан к другой учетной записи");

        var currentUser = await identityService.FindById(request.CurrentUserId);
        if (currentUser == null) return Result.NotFound("Текущий пользователь не найден");

        var currentLogins = await identityService.GetLoginsAsync(currentUser);
        var hasGoogleAlready = currentLogins.Any(l => l.LoginProvider == "Google");
        
        if (hasGoogleAlready) return Result.Error("К вашему аккаунту уже привязан Google. Сначала отвяжите старый.");

        var loginInfo = new UserLoginInfo("Google", payload.Subject, "Google");
        var result = await identityService.AddLogin(currentUser, loginInfo);

        if (!result.Succeeded) return Result.Error("Не удалось привязать Google-аккаунт");

        return Result.Success();
    }
}