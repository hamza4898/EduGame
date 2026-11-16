namespace EduGame.Domain.Entities;

public class SkillSubSection
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    
    public int SkillPathId { get; set; }

    public SkillPath SkillPath { get; set; } = default!;
}