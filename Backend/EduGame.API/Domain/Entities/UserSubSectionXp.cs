namespace EduGame.Domain.Entities;

public class UserSubSectionXp
{
    public int Id { get; set; }
    
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = default!;

    public int SkillSubSectionId { get; set; }

    public SkillSubSection SkillSubSection { get; set; } = default!;

    public int Xp { get; set; } 
}