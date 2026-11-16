using EduGame.Domain.Enums;

namespace EduGame.Application.Courses.CreateCourseWithCommunity;

public record CreateCourseWithCommunityDto(
    string Title,
    string Description,
    bool IsPaid,
    int PriceCoins,
    CourseDifficulty Difficulty,
    int SkillSubSectionId,
    string CommunityName,
    string? CommunityDescription,
    CommunityPrivacyType PrivacyType,
    CommunityMembersLimit MembersLimit);