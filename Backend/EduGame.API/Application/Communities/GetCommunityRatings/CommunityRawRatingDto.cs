namespace EduGame.Application.Communities.GetCommunityRatings;

public class CommunityRawRatingDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int TotalXp { get; set; }

    public int MaxPossibleXp { get; set; }
}