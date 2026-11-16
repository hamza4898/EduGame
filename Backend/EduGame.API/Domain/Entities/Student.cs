namespace EduGame.Domain.Entities;

public class Student : BaseUser
{
    public string? Gender { get; set; } = string.Empty;

    public string? Education { get; set; } = string.Empty;

    public List<StudentXp> ExperienceProgress { get; set; } = new();

    public List<StudentCourse> StudentCourses { get; set; } = new();
}