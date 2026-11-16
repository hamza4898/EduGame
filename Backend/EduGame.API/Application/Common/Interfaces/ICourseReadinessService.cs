using EduGame.Domain.Entities;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Common.Interfaces;

public interface ICourseReadinessService
{
    MyTeacherCourseDto MapToTeacherDto(Course course, string teacherId);
    
    List<string> GetMissingRequirements(Course course);
}