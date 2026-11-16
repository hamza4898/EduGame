using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Users.UpdateMyProfile;

public sealed class UpdateMyProfileCommandHandler(IUserProfileService service)
    : IRequestHandler<UpdateMyProfileCommand, Result<MyBusinessPassportResponseDto>>
{
    public async Task<Result<MyBusinessPassportResponseDto>> Handle(UpdateMyProfileCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId)) return Result.Unauthorized("Не удалось определить пользователя.");

        if (request.Request is null) return Result.Invalid(new ValidationError("Request", "Данные формы профиля пусты."));

        await service.UpdateMyProfileAsync(request.UserId, request.Request, cancellationToken);

        var updatedPassport = await service.GetBusinessPassportAsync(request.UserId, null, cancellationToken);

        if (updatedPassport is null) return Result.NotFound("Не удалось загрузить обновленный профиль пользователя.");

        return Result.Success(updatedPassport);
    }
}