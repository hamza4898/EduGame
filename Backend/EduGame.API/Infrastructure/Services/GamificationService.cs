using EduGame.Application.Common.Interfaces;
using EduGame.Domain.Enums;
using EduGame.Domain.Entities;

namespace EduGame.Infrastructure.Services;

public class GamificationService : IGamificationService
{
    public (int XpGained, int CoinsGained) CalculateLessonRewards(Lesson lesson)
    {
        var difficulty = lesson.Section.Course.Difficulty;
        int maxXpLimit = difficulty switch
        {
            CourseDifficulty.Easy => 300,
            CourseDifficulty.Medium => 700,
            CourseDifficulty.Hard => 2000,
            _ => 150
        };

        int finalXp = lesson.XpReward > maxXpLimit ? maxXpLimit : lesson.XpReward;
        int finalCoins = difficulty switch
        {
            CourseDifficulty.Easy => 5,
            CourseDifficulty.Medium => 15,
            CourseDifficulty.Hard => 40,
            _ => 2
        };

        return (finalXp, finalCoins);
    }

    public int CalculateProgressPercent(int totalLessons, int completedLessons) =>
        totalLessons == 0 ? 100 : (int)((double)completedLessons / totalLessons * 100);
}