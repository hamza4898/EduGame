namespace EduGame.Domain.Entities;

public class Lesson
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public int Order { get; set; }

    public List<LessonBlock> Blocks { get; set; } = [];

    public int XpReward { get; set; }

    public int SectionId { get; set; }
    
    public Section Section { get; set; } = null!;
}