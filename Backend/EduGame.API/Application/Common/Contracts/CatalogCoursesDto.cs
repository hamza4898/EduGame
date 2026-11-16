using EduGame.Application.Courses.GetCatalogCourses;

namespace EduGame.Application.Common.Contracts;

public class CatalogCoursesDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;

    public bool IsPaid { get; set; }

    public int PriceCoins { get; set; }

    public string CommunityId { get; set; } = string.Empty;

    public int SkillSubSectionId { get; set; }

    public SkillSubSectionNestedDto SkillSubSection { get; set; } = null!;

    public CommunityNestedDto Community { get; set; } = null!;
    
    public bool Owned { get; set; }

    public int Progress { get; set; }

    public bool IsLiked { get; set; }

    public bool IsArchived { get; set; }

    public double Rating { get; set; } = 4.8;

    public int StudentsCount { get; set; }
}