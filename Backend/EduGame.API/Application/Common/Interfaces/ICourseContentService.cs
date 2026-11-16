using EduGame.Domain.Entities;
using EduGame.Application.Courses.UpdateCourseContent;

namespace EduGame.Application.Common.Interfaces;

public interface ICourseContentService
{
    string? ValidateContent(Course course, IReadOnlyList<UpdateCourseSectionDto> sections);

    void ApplyChanges(Course course, IReadOnlyList<UpdateCourseSectionDto> sections);
}