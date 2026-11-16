namespace EduGame.Application.Courses.GetStudentCourses;

public class StudentCourseDto
{
    public int CourseId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsPaid { get; set; }

    public int PriceCoins { get; set; }

    public DateTime EnrolledAt { get; set; }

    public int ProgressPercent { get; set; }

    public bool IsCompleted { get; set; }
}