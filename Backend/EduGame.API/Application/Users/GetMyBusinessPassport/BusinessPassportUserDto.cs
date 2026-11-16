namespace EduGame.Application.Users.GetMyBusinessPassport;

public sealed record BusinessPassportUserDto(
    string ExternalId,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string? Gender,
    string? Education,
    string? Subject,
    string? AvatarUrl,
    int GlobalXp,
    int CoinsBalance);