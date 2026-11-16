using EduGame.Domain.Entities;

namespace EduGame.Application.Common.Interfaces;

public interface IRewardBalanceService
{
    public (int XpGained, int CoinsGained) CalculateLessonRewards(Lesson lesson);
}