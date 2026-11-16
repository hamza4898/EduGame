
using EduGame.Domain.Entities;
using EduGame.Application.Common.Interfaces;
using EduGame.Application.Stats.GetSkillPathZones;

namespace EduGame.Infrastructure.Services;

public class RatingCalculatorService : IRatingCalculatorService
{
    private const int MaxExpectedXp = 5000;

    public double CalculateCoursePopularityIndex(double rating, int studentsCount, double avgProgress)
    {
        var baseFromRating = rating * 1.5;
        var logFromStudents = Math.Log(studentsCount + 1, 2);
        var progressWeight = avgProgress * 0.05;

        var index = baseFromRating + logFromStudents + progressWeight;
        
        return Math.Clamp(Math.Round(index, 1), 1.0, 10.0);
    }

    public int CalculateCommunityPowerIndex(double avgProgress, int totalXp)
    {
        if (totalXp <= 0) return 0;
        var logFromXp = Math.Log10(totalXp + 1);
        return (int)Math.Round(avgProgress * logFromXp);
    }

    public List<SkillPathZoneDto> CalculateSkillPathZones(List<SkillPath> skillPaths, List<UserSubSectionXp> userXpRecords)
    {
        if (skillPaths == null) return new List<SkillPathZoneDto>();

        return skillPaths.Select(path =>
        {
            var subSectionIds = path.SubSections.Select(s => s.Id).ToList();

            var totalPathXp = userXpRecords
                .Where(ux => subSectionIds.Contains(ux.SkillSubSectionId))
                .Sum(ux => ux.Xp);

            var progressPct = MaxExpectedXp > 0 ? (int)((double)totalPathXp / MaxExpectedXp * 100) : 0;
            if (progressPct > 100) progressPct = 100;

            return new SkillPathZoneDto
            {
                Id = path.Code, 
                Name = path.Name,
                Icon = path.Icon,
                Color = path.Color,
                TotalXp = totalPathXp,
                ProgressPercent = progressPct
            };
        }).ToList();
    }
}