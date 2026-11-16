namespace EduGame.Application.Courses.GetStudentsRating;

public class StudentRatingResponseDto
{
    public string UserId { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public int GlobalXp { get; set; }

    public int Level { get; set; }
    
    public string Icon { get; set; } = "fa-user-astronaut";
}