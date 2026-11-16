namespace EduGame.Domain.Entities;

public class CourseNews
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public Course Course { get; set; } = null!;
    
    public string AuthorId { get; set; } = string.Empty;
    
    public string Text { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}