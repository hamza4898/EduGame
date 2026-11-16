namespace EduGame.Domain.Entities;

public class StudentLesson
{
    public int Id { get; set; }
    
    public string StudentId { get; set; } = string.Empty;

    public Student Student { get; set; } = null!;
    
    public int LessonId { get; set; }

    public Lesson Lesson { get; set; } = null!;
    
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsCompleted { get; set; } = true;
}