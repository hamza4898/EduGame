namespace EduGame.Domain.Entities;

public class FocusNode
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string Icon { get; set; } = string.Empty;
    
    public int XpThreshold { get; set; }
    
    public string RewardText { get; set; } = string.Empty;
    
    public int Order { get; set; }

    public int SkillSubSectionId { get; set; }

    public SkillSubSection SkillSubSection { get; set; } = default!;
}