namespace EduGame.Application.Courses.GetCourse;

public class BlockNestedDto
{
    public int Id { get; set; }

    public int Type { get; set; }

    public string Content { get; set; } = string.Empty;
    
    public int Order { get; set; }
}