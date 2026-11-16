namespace EduGame.Application.Stats.GetSkillPathZones;

public class SkillPathZoneDto
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public int TotalXp { get; set; }
    
    public int ProgressPercent { get; set; }
}