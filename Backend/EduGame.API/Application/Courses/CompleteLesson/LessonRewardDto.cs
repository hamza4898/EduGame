namespace EduGame.Application.Courses.CompleteLesson;

public class LessonRewardDto
{
    public int XpGained { get; set; }

    public int CoinsGained { get; set; }
    
    public int NewProgressPercent { get; set; }

    public LessonRewardDto(int xpGained, int coinsGained, int newProgressPercent)
    {
        XpGained = xpGained;
        CoinsGained = coinsGained;
        NewProgressPercent = newProgressPercent;
    }
}