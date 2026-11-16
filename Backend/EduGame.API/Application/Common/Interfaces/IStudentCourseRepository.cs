using EduGame.Application.Courses.GetStudentCourses;
using EduGame.Domain.Entities;

namespace EduGame.Application.Common.Interfaces;

public interface IStudentCourseRepository
{
    Task<List<StudentCourseDto>> GetStudentCourses(string studentId, AutoMapper.IConfigurationProvider configurationProvider);

    Task<List<StudentCourse>> GetRawStudentCoursesAsync(string studentId);

    Task<List<int>> GetCompletedLessonIdsAsync(string studentId, int courseId);

    Task<List<int>> GetCourseLessonIdsAsync(int courseId);

    Task<bool> IsAlreadyEnrolledAsync(string studentId, int courseId);
    
    void EnrollStudent(StudentCourse studentCourse);

    Task<Lesson?> GetLessonWithCourseAsync(int lessonId);

    Task<bool> IsLessonAlreadyCompletedAsync(string studentId, int lessonId);

    void CompleteLesson(StudentLesson studentLesson);

    Task<int> GetTotalLessonsInCourseAsync(int courseId);

    Task<int> GetCompletedLessonsCountAsync(string studentId, int courseId);
    
    Task<StudentCourse?> GetStudentCourseAsync(string studentId, int courseId);
}
