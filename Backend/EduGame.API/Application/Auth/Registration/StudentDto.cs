using EduGame.Domain.Enums;

namespace EduGame.Application.Auth.Registration;

public class StudentDto : BaseRegistrationDto
{
    public StudentDto() => UserRole = UserRole.Student;
    
    public string Gender { get; set; } = string.Empty;
    
    public string Education { get; set; } = string.Empty;
}