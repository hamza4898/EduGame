using Microsoft.AspNetCore.Identity;
using EduGame.Domain.Enums;

namespace EduGame.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLogin { get; set; } = DateTime.UtcNow;

    public bool IsAdmin { get; set; }

    public UserRole UserRole { get; set; }

    public bool IsProfileComplete { get; set; } = false;

    public BaseUser? BaseUser { get; set; }
}