using EduGame.Application.Common.Interfaces;
using EduGame.Application.Common.Contracts;
using EduGame.Domain.Enums;

namespace EduGame.Infrastructure.Services;

public class LevelService : ILevelService
{
    public LevelingProgressDto CalculateProgress(int totalXp, string userRole)
    {
        var level = (int)Math.Sqrt(totalXp / 100.0);
        
        switch (level)
        {
            case < 1: level = 1; break;
            case > 100: level = 100; break;
        }

        var xpForCurrent = level == 1 ? 0 : 100 * (level * level);
        var xpForNext = 100 * ((level + 1) * (level + 1));

        var currentXpInLevel = Math.Max(0, totalXp - xpForCurrent);
        var nextLevelXpRequired = xpForNext - xpForCurrent;

        if (level == 100)
        {
            currentXpInLevel = 0;
            nextLevelXpRequired = 0;
        }

        var rankKey = userRole switch
        {
            "Teacher" => GetTeacherRankByLevel(level).ToString(),
            "Student" => GetStudentRankByLevel(level).ToString(),
            _         => GetStudentRankByLevel(level).ToString()
        };
        
        return new LevelingProgressDto
        {
            Level = level,
            RankKey = rankKey, 
            CurrentXpInLevel = currentXpInLevel,
            NextLevelXpRequired = nextLevelXpRequired
        };
    }
    
    private StudentRank GetStudentRankByLevel(int level) => level switch
    {
        <= 10 => StudentRank.Novice,
        <= 20 => StudentRank.Apprentice,
        <= 30 => StudentRank.Adept,
        <= 40 => StudentRank.Virtuoso,
        <= 50 => StudentRank.Master,
        <= 60 => StudentRank.Expert,
        <= 70 => StudentRank.Magister,
        <= 80 => StudentRank.Archmage,
        <= 90 => StudentRank.Grandmaster,
        _     => StudentRank.Legend
    };

    private TeacherRank GetTeacherRankByLevel(int level) => level switch
    {
        <= 10 => TeacherRank.Assistant,
        <= 20 => TeacherRank.Instructor,
        <= 30 => TeacherRank.Lector,
        <= 40 => TeacherRank.Curator,
        <= 50 => TeacherRank.Mentor,
        <= 60 => TeacherRank.Professor,
        <= 70 => TeacherRank.AcademyMagister,
        <= 80 => TeacherRank.Dean,
        <= 90 => TeacherRank.KnowledgeArchitect,
        _     => TeacherRank.Grandmaster
    };
}
