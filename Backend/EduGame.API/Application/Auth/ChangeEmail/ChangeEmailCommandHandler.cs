using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Application.Auth.ChangeEmail;

public class ChangeEmailCommandHandler(IIdentityService identityService) : IRequestHandler<ChangeEmailCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ChangeEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await identityService.FindById(request.UserId);

        if (user == null) return Result.NotFound();

        var isPasswordValid = await identityService.CheckPassword(user, request.CurrentPassword);

        if (!isPasswordValid) return Result.Error("Неверный пароль");

        await identityService.SetEmail(user, request.NewEmail);

        user.EmailConfirmed = true;
        await identityService.Update(user);

        return Result.Success("Email успешно изменен");
    }
}