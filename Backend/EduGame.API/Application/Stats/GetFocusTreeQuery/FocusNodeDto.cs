namespace EduGame.Application.Stats.GetFocusTree;

public class FocusNodeDto
{
    public string Name { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public int XpThreshold { get; set; }

    public string RewardText { get; set; } = string.Empty;

    public int Order { get; set; }
    
    public int SkillSubSectionId { get; set; }

    public string SubSectionName { get; set; } = string.Empty;

    public int CurrentUserXp { get; set; }
}