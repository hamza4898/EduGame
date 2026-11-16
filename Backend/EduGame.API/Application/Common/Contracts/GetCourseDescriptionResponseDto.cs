namespace EduGame.Application.Common.Contracts;

public record GetCourseDescriptionResponseDto(
    string CourseName,
    string CourseDescription,
    string SkillPath,
    string SkillSubSection,
    int SkillSubSectionId,
    int Difficulty,
    List<CourseContentSectionDto> Sections,
    bool IsPaid,
    int PriceCoins,
    bool IsPublished,
    bool CertificateEnabled,
    string CertificateSigner
);