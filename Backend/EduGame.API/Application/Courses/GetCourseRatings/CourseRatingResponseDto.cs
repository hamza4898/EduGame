namespace EduGame.Application.Courses.GetCourseRatings;

public class CourseRatingResponseDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public double PopIndex { get; set; }
}