using EduGame.Domain.Entities;

namespace EduGame.Application.Common.Interfaces;

public interface IGamificationService
{
    (int XpGained, int CoinsGained) CalculateLessonRewards(Lesson lesson);
    int CalculateProgressPercent(int totalLessons, int completedLessons);
}