namespace EduGame.Domain.Entities;

public class StudentCourse
{
    public Student Student { get; set; } = null!;

    public string StudentId { get; set; } = string.Empty;

    public Course Course { get; set; } = null!;

    public int CourseId { get; set; }

    public DateTime EnrolledAt { get; set; }

    public int ProgressPercent { get; set; }

    public bool IsCompleted { get; set; }
}