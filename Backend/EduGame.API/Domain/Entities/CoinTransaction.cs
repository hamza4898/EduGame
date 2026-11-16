namespace EduGame.Domain.Entities;

public class CoinTransaction
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public int Amount { get; set; }

    public string Type { get; set; } = string.Empty;

    public int CourseId { get; set; }

    public string CounterpartyId { get; set; } = string.Empty;

    public Guid TransactionGroupId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}