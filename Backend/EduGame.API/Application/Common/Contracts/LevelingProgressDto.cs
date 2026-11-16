namespace EduGame.Application.Common.Contracts;

public class LevelingProgressDto
{
    public int Level { get; set; }

    public string RankKey { get; set; } = string.Empty;

    public string Badge { get; set; } = string.Empty;

    public int CurrentXpInLevel { get; set; }

    public int NextLevelXpRequired { get; set; }
}