using EduGame.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EduGame.Infrastructure.Data;

public static class DatabaseSeeder
{
    private static readonly (string Code, string Name, string Icon, string Color, string[] SubSections)[] Paths =
    [
        ("it", "Путь IT", "fa-laptop-code", "#06b6d4", ["Разработка", "Данные и AI", "Инфраструктура и Кибербез"]),
        ("creator", "Путь Творца", "fa-palette", "#ec4899", ["Визуальный дизайн", "Иллюстрация и Арт", "Медиа и Контент"]),
        ("polyglot", "Путь Полиглота", "fa-language", "#a855f7", ["Базовое владение", "Продвинутый язык", "Коммуникация и культура"]),
        ("strategist", "Путь Стратега", "fa-chess", "#f59e0b", ["Предпринимательство", "Менеджмент и Лидерство", "Маркетинг и Продажи"]),
        ("architect", "Путь Зодчего", "fa-compass-drafting", "#3b82f6", ["Архитектура", "Инженерия и Конструкции", "Умная среда и Инновации"]),
        ("growth", "Путь Восхождения", "fa-arrow-up-right-dots", "#10b981", ["Мышление и обучение", "Эмоции и Коммуникация", "Цели и Продуктивность"])
    ];

    public static async Task SeedAsync(EduGameDbContext context, CancellationToken cancellationToken = default)
    {
        await context.Database.ExecuteSqlRawAsync(
            """SELECT setval(pg_get_serial_sequence('"SkillPaths"', 'Id'), COALESCE(MAX("Id"), 1), MAX("Id") IS NOT NULL) FROM "SkillPaths";""",
            cancellationToken);
        await context.Database.ExecuteSqlRawAsync(
            """SELECT setval(pg_get_serial_sequence('"SkillSubSections"', 'Id'), COALESCE(MAX("Id"), 1), MAX("Id") IS NOT NULL) FROM "SkillSubSections";""",
            cancellationToken);

        foreach (var pathData in Paths)
        {
            var path = await context.SkillPaths
                .Include(item => item.SubSections)
                .FirstOrDefaultAsync(item => item.Code == pathData.Code, cancellationToken);

            if (path is null)
            {
                path = new SkillPath
                {
                    Code = pathData.Code,
                    Name = pathData.Name,
                    Icon = pathData.Icon,
                    Color = pathData.Color
                };
                context.SkillPaths.Add(path);
            }
            else
            {
                path.Name = pathData.Name;
                path.Icon = pathData.Icon;
                path.Color = pathData.Color;
            }

            var existingNames = path.SubSections.Select(section => section.Name).ToHashSet(StringComparer.Ordinal);
            foreach (var name in pathData.SubSections.Where(name => !existingNames.Contains(name)))
                path.SubSections.Add(new SkillSubSection { Name = name });
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}