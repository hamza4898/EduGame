using EduGame.Domain.Enums;

namespace EduGame.Application.Auth.Registration;

public abstract class BaseRegistrationDto
{
    public string ClientRoute { get; set; } = string.Empty;
    
    public string UserName { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public string PhoneNumber { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string Motivation { get; set; } = string.Empty;

    public UserRole UserRole { get; set; }
}