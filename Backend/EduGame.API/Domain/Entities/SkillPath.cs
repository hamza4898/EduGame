namespace EduGame.Domain.Entities;

public class SkillPath
{
    public int Id { get; set; }
    
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    
    public string Icon { get; set; } = string.Empty;
    
    public string Color { get; set; } = string.Empty;
    
    public ICollection<SkillSubSection> SubSections { get; set; } = new List<SkillSubSection>();
}