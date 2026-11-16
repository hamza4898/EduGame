namespace EduGame.Domain.Entities;

public class CourseTeacherAccess
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public Course Course { get; set; } = null!;

    public string TeacherId { get; set; } = string.Empty;

    public string Role { get; set; } = "Assistant";
}