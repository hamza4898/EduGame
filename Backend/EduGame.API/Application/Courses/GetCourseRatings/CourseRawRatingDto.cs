namespace EduGame.Application.Courses.GetCourseRatings;

public class CourseRawRatingDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public double Rating { get; set; }

    public int StudentsCount { get; set; }
    
    public double AvgProgress { get; set; }
}