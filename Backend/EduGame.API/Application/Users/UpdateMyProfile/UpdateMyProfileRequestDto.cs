namespace EduGame.Application.Users.UpdateMyProfile;

public sealed record UpdateMyProfileRequestDto(
    string FirstName,
    string LastName,
    DateOnly? DateOfBirth,
    string? Gender,
    string? Education,
    string? Subject,
    IFormFile? Avatar);