using Microsoft.EntityFrameworkCore;
using EduGame.Infrastructure.Data;
using EduGame.Application.Common.Interfaces;
using EduGame.Domain.Entities;
using EduGame.Application.Stats.GetFocusTree;

namespace EduGame.Infrastructure.Repositories;

public class SkillTreeRepository(EduGameDbContext context) : ISkillTreeRepository
{
    private readonly EduGameDbContext _context = context;

    public async Task<SkillSubSection?> GetSkillSubSection(int id) =>
        await _context.SkillSubSections
            .AsNoTracking()
            .FirstOrDefaultAsync(section => section.Id == id);

    public async Task<List<SkillPath>> GetSkillPathsWithSubSectionsAsync() =>
        await _context.SkillPaths
            .Include(p => p.SubSections)
            .AsNoTracking()
            .ToListAsync();

    public async Task<List<UserSubSectionXp>> GetUserSubSectionXpAsync(string userId) =>
        await _context.UserSubSectionXps
            .Where(ux => ux.UserId == userId)
            .AsNoTracking()
            .ToListAsync();

    public async Task<Dictionary<int, int>> GetUserXpMapByPathCodeAsync(string userId, string pathCode) =>
        await _context.UserSubSectionXps
            .AsNoTracking()
            .Where(ux => ux.UserId == userId && ux.SkillSubSection.SkillPath.Code == pathCode)
            .ToDictionaryAsync(ux => ux.SkillSubSectionId, ux => ux.Xp);

    public async Task<int> GetPathIdByCodeAsync(string pathCode) =>
        await _context.SkillPaths
            .AsNoTracking()
            .Where(p => p.Code == pathCode)
            .Select(p => p.Id)
            .FirstOrDefaultAsync();

    public async Task<Dictionary<int, int>> GetUserXpMapByPathIdAsync(string userId, int pathId) =>
        await _context.UserSubSectionXps
            .AsNoTracking()
            .Where(ux => ux.UserId == userId && ux.SkillSubSection.SkillPathId == pathId)
            .ToDictionaryAsync(ux => ux.SkillSubSectionId, ux => ux.Xp);

    public async Task<List<FocusNodeDto>> GetFocusNodesByPathIdAsync(int pathId) =>
        await _context.FocusNodes
            .AsNoTracking()
            .Where(n => n.SkillSubSection.SkillPathId == pathId) 
            .OrderBy(n => n.SkillSubSectionId)
            .ThenBy(n => n.Order)
            .Select(n => new FocusNodeDto
            {
                Name = n.Name,
                Icon = n.Icon,
                XpThreshold = n.XpThreshold,
                RewardText = n.RewardText,
                SubSectionName = n.SkillSubSection.Name,
                SkillSubSectionId = n.SkillSubSectionId
            })
            .ToListAsync();

    public async Task AwardSubSectionXpAsync(string userId, int skillSubSectionId, int xp)
    {
        var existingXp = await _context.UserSubSectionXps
            .FirstOrDefaultAsync(ux => ux.UserId == userId && ux.SkillSubSectionId == skillSubSectionId);

        if (existingXp != null)
        {
            existingXp.Xp += xp;
        }
        else
        {
            await _context.UserSubSectionXps.AddAsync(new UserSubSectionXp 
            { 
                UserId = userId, 
                SkillSubSectionId = skillSubSectionId, 
                Xp = xp 
            });
        }
    }
}