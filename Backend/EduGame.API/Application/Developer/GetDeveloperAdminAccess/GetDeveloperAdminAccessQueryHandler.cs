using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Application.Developer.GetDeveloperAdminAccess;

internal sealed class GetDeveloperAdminAccessQueryHandler(
    IUserRepository userRepository
    ) : IRequestHandler<GetDeveloperAdminAccessQuery, Result<bool>>
{
    public async Task<Result<bool>> Handle(GetDeveloperAdminAccessQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId)) return Result.Unauthorized("Пользователь не авторизован.");

        var isDeveloper = await userRepository.IsInRoleAsync(request.UserId, "Developer", cancellationToken);
        var isAdmin = await userRepository.IsInRoleAsync(request.UserId, "Admin", cancellationToken);

        if (!isDeveloper && !isAdmin) return Result.Forbidden("У вас нет прав доступа к панели разработчика.");

        return Result.Success(true);
    }
}