using EduGame.Domain.Enums;

namespace EduGame.Domain.Entities;

public class StudentXp
{
    public int Id { get; set; }
    
    public string StudentId { get; set; } = string.Empty;

    public MainPath Path { get; set; }

    public SubSphereType SubSphere { get; set; }
    
    public int Xp { get; set; }
}