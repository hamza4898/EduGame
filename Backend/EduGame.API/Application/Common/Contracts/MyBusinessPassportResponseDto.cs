using EduGame.Application.Users.GetMyBusinessPassport;

namespace EduGame.Application.Common.Contracts;

public sealed record MyBusinessPassportResponseDto(
    BusinessPassportUserDto User,
    string UserName,
    LevelingProgressDto LevelingProgress,
    string UserRole);