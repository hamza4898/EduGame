using EduGame.Domain.Enums;

namespace EduGame.Application.Common.Contracts;

public record MyTeacherCourseDto(
    int Id,
    string Title,
    string Description,
    bool IsPaid,
    int PriceCoins,
    CourseDifficulty Difficulty,
    int SkillSubSectionId,
    string SkillSubSectionName,
    string SkillPathName,
    string CommunityName,
    bool IsOwnedByTeacher,
    bool IsPublished,
    int ModuleCount,
    int LessonCount,
    int NewsCount,
    bool CertificateEnabled,
    bool IsReadyToPublish,
    int ReadinessPercent);