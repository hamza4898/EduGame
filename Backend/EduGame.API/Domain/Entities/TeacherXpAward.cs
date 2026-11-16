namespace EduGame.Domain.Entities;

public class TeacherXpAward
{
    public int Id { get; set; }

    public string TeacherId { get; set; } = string.Empty;

    public int CourseId { get; set; }

    public string? StudentId { get; set; }

    public string AwardKey { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public int Xp { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}