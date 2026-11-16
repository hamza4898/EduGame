using System.Text.Json;
using EduGame.Domain.Entities;
using EduGame.Domain.Enums;
using EduGame.Application.Common.Interfaces;
using EduGame.Application.Courses.UpdateCourseContent;

namespace EduGame.Infrastructure.Services;

public sealed class CourseContentService : ICourseContentService
{
    public string? ValidateContent(Course course, IReadOnlyList<UpdateCourseSectionDto> sections)
    {
        if (sections.Count == 0)
            return "Содержание курса не может быть пустым.";

        var existingSections = course.Sections.ToDictionary(section => section.Id);
        var existingLessons = course.Sections
            .SelectMany(section => section.Lessons)
            .ToDictionary(lesson => lesson.Id);
            
        var requestedSectionIds = new HashSet<int>();
        var requestedLessonIds = new HashSet<int>();
        var maximumXp = GetMaximumXp(course.Difficulty);

        foreach (var section in sections)
        {
            if (section is null)
                return "Содержимое модуля некорректно.";

            var title = section.Title?.Trim();
            if (string.IsNullOrWhiteSpace(title) || title.Length > 250)
                return "Название модуля должно содержать от 1 до 250 символов.";

            if (section.Id < 0
                || (section.Id > 0 && !existingSections.ContainsKey(section.Id))
                || (section.Id > 0 && !requestedSectionIds.Add(section.Id)))
                return "Модуль не принадлежит этому курсу или указан повторно.";

            if (section.Lessons is null)
                return "Список уроков модуля некорректен.";

            foreach (var lesson in section.Lessons)
            {
                if (lesson is null)
                    return "Содержимое урока некорректно.";

                var lessonTitle = lesson.Title?.Trim();
                if (string.IsNullOrWhiteSpace(lessonTitle) || lessonTitle.Length > 250)
                    return "Название урока должно содержать от 1 до 250 символов.";

                if (lesson.XpReward < 0 || lesson.XpReward > maximumXp)
                    return $"Награда урока должна быть от 0 до {maximumXp} XP для этой сложности курса.";

                if (lesson.Blocks is null)
                    return "Список материалов урока некорректен.";

                if (lesson.Id < 0)
                    return "Идентификатор урока некорректен.";

                if (lesson.Id > 0)
                {
                    if (!existingLessons.TryGetValue(lesson.Id, out var existingLesson)
                        || existingLesson.SectionId != section.Id
                        || !requestedLessonIds.Add(lesson.Id))
                        return "Урок не принадлежит этому модулю курса или указан повторно.";
                }

                foreach (var block in lesson.Blocks)
                {
                    if (block is null)
                        return "Содержимое материала урока некорректно.";
                    if (!Enum.IsDefined(typeof(BlockType), block.Type))
                        return "Тип материала урока не поддерживается.";

                    var type = (BlockType)block.Type;
                    var content = block.Content ?? string.Empty;
                    if (content.Length > 100_000)
                        return "Слишком большой материал урока.";

                    if (type is BlockType.Image or BlockType.Video && !IsSupportedMediaUrl(content, type))
                        return "Для фото и видео укажите поддерживаемую ссылку HTTPS.";

                    if (type is BlockType.Quiz or BlockType.Assignment && !IsValidTask(content, type))
                        return "Проверьте формат квиза или задания.";
                }
            }
        }

        if (!existingSections.Keys.All(requestedSectionIds.Contains)
            || !existingLessons.Keys.All(requestedLessonIds.Contains))
        {
            return "Удаление существующих модулей и уроков пока не поддерживается.";
        }

        return null;
    }

    public void ApplyChanges(Course course, IReadOnlyList<UpdateCourseSectionDto> sections)
    {
        var existingSections = course.Sections.ToDictionary(section => section.Id);
        var existingLessons = course.Sections
            .SelectMany(section => section.Lessons)
            .ToDictionary(lesson => lesson.Id);

        for (var sectionIndex = 0; sectionIndex < sections.Count; sectionIndex++)
        {
            var sectionDto = sections[sectionIndex];
            var section = sectionDto.Id == 0 ? new Section() : existingSections[sectionDto.Id];

            if (sectionDto.Id == 0)
                course.Sections.Add(section);

            section.Title = sectionDto.Title.Trim();
            section.Order = sectionIndex;

            for (var lessonIndex = 0; lessonIndex < sectionDto.Lessons.Count; lessonIndex++)
            {
                var lessonDto = sectionDto.Lessons[lessonIndex];
                var lesson = lessonDto.Id == 0 ? new Lesson() : existingLessons[lessonDto.Id];

                if (lessonDto.Id == 0)
                    section.Lessons.Add(lesson);

                lesson.Title = lessonDto.Title.Trim();
                lesson.Order = lessonIndex;
                lesson.XpReward = lessonDto.XpReward;
                UpdateBlocks(lesson, lessonDto.Blocks);
            }
        }
    }

    private void UpdateBlocks(Lesson lesson, IReadOnlyList<UpdateCourseBlockDto> blocks)
    {
        var existingBlocks = lesson.Blocks.OrderBy(block => block.Order).ToList();

        for (var blockIndex = 0; blockIndex < blocks.Count; blockIndex++)
        {
            var blockDto = blocks[blockIndex];
            if (blockIndex < existingBlocks.Count)
            {
                var block = existingBlocks[blockIndex];
                block.Type = (BlockType)blockDto.Type;
                block.Content = blockDto.Content ?? string.Empty;
                block.Order = blockIndex;
            }
            else
            {
                lesson.Blocks.Add(new LessonBlock
                {
                    Type = (BlockType)blockDto.Type,
                    Content = blockDto.Content ?? string.Empty,
                    Order = blockIndex
                });
            }
        }

        foreach (var removedBlock in existingBlocks.Skip(blocks.Count))
            lesson.Blocks.Remove(removedBlock);
    }

    private int GetMaximumXp(CourseDifficulty difficulty) =>
        difficulty switch
        {
            CourseDifficulty.Easy => 300,
            CourseDifficulty.Medium => 700,
            CourseDifficulty.Hard => 2000,
            _ => 150
        };

    private bool IsSupportedMediaUrl(string value, BlockType type)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)
            || uri.Scheme != Uri.UriSchemeHttps
            || !string.IsNullOrEmpty(uri.UserInfo))
        {
            return false;
        }

        if (type == BlockType.Image)
            return true;

        return uri.Host.Equals("youtube.com", StringComparison.OrdinalIgnoreCase)
            || uri.Host.Equals("www.youtube.com", StringComparison.OrdinalIgnoreCase)
            || uri.Host.Equals("m.youtube.com", StringComparison.OrdinalIgnoreCase)
            || uri.Host.Equals("youtube-nocookie.com", StringComparison.OrdinalIgnoreCase)
            || uri.Host.Equals("www.youtube-nocookie.com", StringComparison.OrdinalIgnoreCase)
            || uri.Host.Equals("youtu.be", StringComparison.OrdinalIgnoreCase)
            || uri.Host.Equals("vimeo.com", StringComparison.OrdinalIgnoreCase)
            || uri.Host.Equals("player.vimeo.com", StringComparison.OrdinalIgnoreCase);
    }

    private bool IsValidTask(string content, BlockType type)
    {
        try
        {
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
                return false;

            if (type == BlockType.Quiz)
            {
                return root.TryGetProperty("question", out var question)
                    && question.ValueKind == JsonValueKind.String
                    && !string.IsNullOrWhiteSpace(question.GetString())
                    && root.TryGetProperty("options", out var options)
                    && options.ValueKind == JsonValueKind.Array
                    && options.GetArrayLength() >= 2
                    && options.GetArrayLength() <= 6
                    && options.EnumerateArray().All(option =>
                        option.ValueKind == JsonValueKind.String
                        && !string.IsNullOrWhiteSpace(option.GetString()))
                    && root.TryGetProperty("answer", out var answer)
                    && answer.TryGetInt32(out var answerIndex)
                    && answerIndex >= 0
                    && answerIndex < options.GetArrayLength();
            }

            return root.TryGetProperty("prompt", out var prompt)
                && prompt.ValueKind == JsonValueKind.String
                && !string.IsNullOrWhiteSpace(prompt.GetString());
        }
        catch (JsonException)
        {
            return false;
        }
    }
}