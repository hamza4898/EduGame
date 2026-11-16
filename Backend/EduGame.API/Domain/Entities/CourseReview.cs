namespace EduGame.Domain.Entities;

public class CourseReview
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public Course Course { get; set; } = null!;

    public string StudentId { get; set; } = string.Empty;

    public int Stars { get; set; }

    public string Text { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}