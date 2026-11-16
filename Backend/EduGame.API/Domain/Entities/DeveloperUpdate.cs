namespace EduGame.Domain.Entities;

public sealed class DeveloperUpdate
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Category { get; set; } = "Обновление";

    public string Content { get; set; } = string.Empty;

    public string AuthorSignature { get; set; } = "Хамза — Главный разработчик EduGame";

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}