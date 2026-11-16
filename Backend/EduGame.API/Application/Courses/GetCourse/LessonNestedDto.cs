namespace EduGame.Application.Courses.GetCourse;

public class LessonNestedDto
{
    public int Id { get; set; }
    
    public string Title { get; set; } = string.Empty;

    public int Order { get; set; }

    public string Status { get; set; } = "locked";
    
    public List<BlockNestedDto> Blocks { get; set; } = [];
}