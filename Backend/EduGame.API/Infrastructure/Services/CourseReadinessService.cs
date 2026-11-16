using EduGame.Application.Common.Interfaces;
using EduGame.Application.Common.Contracts;
using EduGame.Domain.Entities;
using EduGame.Domain.Enums;

namespace EduGame.Infrastructure.Services;

public sealed class CourseReadinessService : ICourseReadinessService
{
    public MyTeacherCourseDto MapToTeacherDto(Course course, string teacherId)
    {
        var missingRequirements = GetMissingRequirements(course);
        
        return new MyTeacherCourseDto(
            course.Id,
            course.Title,
            course.Description,
            course.IsPaid,
            course.PriceCoins,
            course.Difficulty,
            course.SkillSubSectionId,
            course.SkillSubSection?.Name ?? string.Empty,
            course.SkillSubSection?.SkillPath?.Name ?? string.Empty,
            course.Community?.CommunityName ?? string.Empty,
            course.Community?.TeacherId == teacherId,
            course.IsPublished,
            course.Sections?.Count ?? 0,
            course.Sections?.Sum(section => section.Lessons?.Count ?? 0) ?? 0,
            course.News?.Count ?? 0,
            course.CertificateEnabled,
            missingRequirements.Count == 0,
            (int)Math.Round((7 - missingRequirements.Count) / 7d * 100));
    }

    public List<string> GetMissingRequirements(Course course)
    {
        var missing = new List<string>();
        if (course.Sections is null) return missing;

        var lessons = course.Sections.SelectMany(section => section.Lessons ?? []).ToList();
        
        if (string.IsNullOrWhiteSpace(course.Title) || course.Title.Trim().Length < 5) missing.Add("title");
        if (string.IsNullOrWhiteSpace(course.Description) || course.Description.Trim().Length < 20) missing.Add("description");
        if (lessons.Count < 3) missing.Add("lessons");
        
        if (lessons.Count(lesson => lesson.Blocks is not null && lesson.Blocks.Any(block => !string.IsNullOrWhiteSpace(block.Content))) < 2)
            missing.Add("content");
            
        if (!lessons.SelectMany(lesson => lesson.Blocks ?? []).Any(block => block.Type == BlockType.Quiz))
            missing.Add("task");
            
        if (course.News is null || course.News.Count == 0) missing.Add("news");
        if (course.IsPaid ? course.PriceCoins <= 0 : course.PriceCoins != 0) missing.Add("price");
        
        return missing;
    }
}