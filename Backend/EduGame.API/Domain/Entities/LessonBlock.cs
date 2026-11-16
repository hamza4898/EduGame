using EduGame.Domain.Enums;

namespace EduGame.Domain.Entities;

public class LessonBlock
{
    public int Id { get; set; }

    public BlockType Type { get; set; }

    public string Content { get; set; } = string.Empty;

    public int Order { get; set; }

    public int LessonId { get; set; }
    
    public Lesson Lesson { get; set; } = null!;
}