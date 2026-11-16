using EduGame.Domain.Enums;

namespace EduGame.Application.Auth.CompleteProfile;

public record CompleteProfileDto(UserRole SelectedRole, string UserName, string FirstName, string LastName, DateTime DateOfBirth);