namespace EduGame.Domain.Entities;

public class CourseLike
{
    public string UserId { get; set; } = string.Empty;

    public BaseUser User { get; set; } = null!;

    public int CourseId { get; set; }

    public Course Course { get; set; } = null!; 

    public DateTime LikedAt { get; set; } = DateTime.UtcNow;
}