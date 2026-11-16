using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Auth.Login;

public class LoginUserCommandHandler(
    IIdentityService identityService,
    ILogger<LoginUserCommandHandler> logger,
    IAuthService authService
) : IRequestHandler<LoginUserCommand, Result<LoginResponseDto>>
{
    public async Task<Result<LoginResponseDto>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var userLogin = await identityService.FindByEmailAsync(request.Email);

        if (userLogin == null)
        {
            logger.LogWarning("Not found a user with Email: {Email}", request.Email);
            return Result.NotFound("Неверный логин или пароль!");  
        }

        var userSignInResult = await identityService.CheckPasswordSignInAsync(userLogin, request.Password, lockoutOnFailure: true);

        if (!userSignInResult.Succeeded)
        {
            return userSignInResult switch
            {
                { IsLockedOut: true } => Result.Forbidden("Слишком много попыток. Попробуйте через 10 минут!"),
                { IsNotAllowed: true } => Result.Forbidden("Email не подтвержден!"),
                _ => Result.Unauthorized("Неверный логин или пароль!")
            };    
        }

        var isTwoFactorEnabled = userLogin.TwoFactorEnabled;

        var tokens = await authService.GenerateSession(userLogin);

        return Result.Success(new LoginResponseDto(tokens.AccessToken, tokens.RefreshToken, isTwoFactorEnabled));
    }
}