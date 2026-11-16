using EduGame.Domain.Enums;

namespace EduGame.Application.Auth.Registration;

public class TeacherDto : BaseRegistrationDto
{
    public TeacherDto() => UserRole = UserRole.Teacher;

    public string Gender { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;
}