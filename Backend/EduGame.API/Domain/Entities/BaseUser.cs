namespace EduGame.Domain.Entities;

public class BaseUser
{
    public string Id { get; set; } = string.Empty;

    public ApplicationUser ApplicationUser { get; set; } = null!;

    public string FirstName { get; set; } = string.Empty;
    
    public string LastName { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public string? AvatarUrl { get; set; }

    public int GlobalXp { get; set; }

    public int CoinsBalance { get; set; }
}