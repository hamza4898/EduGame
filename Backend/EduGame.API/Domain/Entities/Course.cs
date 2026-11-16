using EduGame.Domain.Enums;

namespace EduGame.Domain.Entities;

public class Course
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsPaid { get; set; }

    public int PriceCoins { get; set; }

    public bool IsPublished { get; set; }

    public bool CertificateEnabled { get; set; }

    public string CertificateSigner { get; set; } = string.Empty;

    public int CommunityId { get; set; }

    public CourseDifficulty Difficulty { get; set; }

    public Community Community { get; set; } = null!;

    public List<Section> Sections { get; set; } = [];

    public List<CourseNews> News { get; set; } = [];

    public List<CourseReview> Reviews { get; set; } = [];

    public List<CoursePromoCode> PromoCodes { get; set; } = [];

    public List<CourseTeacherAccess> TeacherAccess { get; set; } = [];

    public int SkillSubSectionId { get; set; }
    
    public SkillSubSection SkillSubSection { get; set; } = default!;
}