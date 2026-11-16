namespace EduGame.Domain.Entities;

public class CoursePromoCode
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public Course Course { get; set; } = null!;

    public string Code { get; set; } = string.Empty;

    public int DiscountPercent { get; set; }
}