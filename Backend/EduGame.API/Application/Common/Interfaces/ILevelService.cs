using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Common.Interfaces;

public interface ILevelService
{
    LevelingProgressDto CalculateProgress(int totalXp, string userRole);
}