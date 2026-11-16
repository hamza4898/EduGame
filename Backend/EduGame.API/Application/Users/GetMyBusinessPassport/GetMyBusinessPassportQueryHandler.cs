using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using EduGame.Domain.Entities;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Users.GetMyBusinessPassport;

public class GetMyBusinessPassportQueryHandler(
    IUserRepository businessRepository,
    IIdentityRepository identityRepository,
    ILevelService levelService
)
    : IRequestHandler<GetMyBusinessPassportQuery, Result<MyBusinessPassportResponseDto>>
{
    public async Task<Result<MyBusinessPassportResponseDto>> Handle(GetMyBusinessPassportQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.UserRole))
            return Result.Unauthorized("Не удалось определить пользователя.");

        var user = request.UserRole switch
        {
            "Student" => (BaseUser?)await businessRepository.GetStudentById(request.UserId),
            "Teacher" => (BaseUser?)await businessRepository.GetTeacherById(request.UserId),
            _ => null
        };

        if (user == null) return Result.NotFound("Профиль пользователя не найден в базе данных!");

        var userName = await identityRepository.GetUserName(request.UserId);

        if (userName == null) return Result.NotFound("Никнейм пользователя не найден");

        var userProgress = levelService.CalculateProgress(user.GlobalXp, request.UserRole);
        var profile = new BusinessPassportUserDto(
            user.Id,
            user.FirstName,
            user.LastName,
            DateOnly.FromDateTime(user.DateOfBirth),
            user switch
            {
                Student student => student.Gender,
                Teacher teacher => teacher.Gender,
                _ => null
            },
            user is Student studentUser ? studentUser.Education : null,
            user is Teacher teacherUser ? teacherUser.Subject : null,
            user.AvatarUrl,
            user.GlobalXp,
            user.CoinsBalance);

        return Result.Success(new MyBusinessPassportResponseDto(profile, userName!, userProgress, request.UserRole));
    }
}