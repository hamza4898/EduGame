namespace EduGame.Application.Courses.GetCatalogCourses;

public class SkillSubSectionNestedDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int SkillPathId { get; set; }
    
    public SkillPathNestedDto SkillPath { get; set; } = null!;
}