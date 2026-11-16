using EduGame.Application.Stats.GetFocusTree;
using EduGame.Domain.Entities;

namespace EduGame.Application.Common.Interfaces;

public interface ISkillTreeRepository
{
    Task<SkillSubSection?> GetSkillSubSection(int id);

    Task<List<SkillPath>> GetSkillPathsWithSubSectionsAsync();

    Task<List<UserSubSectionXp>> GetUserSubSectionXpAsync(string userId);

    Task<Dictionary<int, int>> GetUserXpMapByPathCodeAsync(string userId, string pathCode);

    Task<int> GetPathIdByCodeAsync(string pathCode);

    Task<Dictionary<int, int>> GetUserXpMapByPathIdAsync(string userId, int pathId);

    Task<List<FocusNodeDto>> GetFocusNodesByPathIdAsync(int pathId);

    Task AwardSubSectionXpAsync(string userId, int skillSubSectionId, int xp);
}