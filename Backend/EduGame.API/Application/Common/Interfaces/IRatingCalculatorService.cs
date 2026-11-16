using EduGame.Application.Stats.GetSkillPathZones;
using EduGame.Domain.Entities;

namespace EduGame.Application.Common.Interfaces;

public interface IRatingCalculatorService
{
    double CalculateCoursePopularityIndex(double rating, int studentsCount, double avgProgress);
    
    int CalculateCommunityPowerIndex(double avgProgress, int totalXp);

    List<SkillPathZoneDto> CalculateSkillPathZones(List<SkillPath> skillPaths, List<UserSubSectionXp> userXpRecords);
}