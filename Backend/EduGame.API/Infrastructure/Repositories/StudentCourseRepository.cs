using AutoMapper.QueryableExtensions;
using EduGame.Infrastructure.Data;
using EduGame.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using EduGame.Application.Common.Interfaces;
using EduGame.Application.Courses.GetStudentCourses;

namespace EduGame.Infrastructure.Repositories;

public class StudentCourseRepository(EduGameDbContext context) : IStudentCourseRepository
{
    public async Task<List<StudentCourseDto>> GetStudentCourses(string studentId, AutoMapper.IConfigurationProvider configurationProvider) =>
        await context.StudentCourses
            .Where(sc => sc.StudentId == studentId)
            .ProjectTo<StudentCourseDto>(configurationProvider)
            .ToListAsync();

    public async Task<List<StudentCourse>> GetRawStudentCoursesAsync(string studentId) =>
        await context.StudentCourses
            .AsNoTracking()
            .Where(sc => sc.StudentId == studentId)
            .ToListAsync();

    public async Task<List<int>> GetCompletedLessonIdsAsync(string studentId, int courseId) =>
        await context.StudentLessons
            .AsNoTracking()
            .Where(sl => sl.StudentId == studentId
                && sl.IsCompleted
                && sl.Lesson.Section.CourseId == courseId)
            .Select(sl => sl.LessonId)
            .ToListAsync();

    public async Task<List<int>> GetCourseLessonIdsAsync(int courseId) =>
        await context.Lessons
            .AsNoTracking()
            .Where(lesson => lesson.Section.CourseId == courseId)
            .OrderBy(lesson => lesson.Section.Order)
            .ThenBy(lesson => lesson.Order)
            .Select(lesson => lesson.Id)
            .ToListAsync();

    public async Task<bool> IsAlreadyEnrolledAsync(string studentId, int courseId) =>
        await context.StudentCourses.AnyAsync(sc => sc.StudentId == studentId && sc.CourseId == courseId);

    public void EnrollStudent(StudentCourse studentCourse) =>
        context.StudentCourses.Add(studentCourse);

    public async Task<Lesson?> GetLessonWithCourseAsync(int lessonId) =>
        await context.Lessons
            .Include(lesson => lesson.Blocks)
            .Include(l => l.Section)
                .ThenInclude(section => section.Course)
            .FirstOrDefaultAsync(l => l.Id == lessonId);

    public async Task<bool> IsLessonAlreadyCompletedAsync(string studentId, int lessonId) =>
        await context.StudentLessons.AnyAsync(sl => sl.StudentId == studentId && sl.LessonId == lessonId && sl.IsCompleted);

    public void CompleteLesson(StudentLesson studentLesson) =>
        context.StudentLessons.Add(studentLesson);

    public async Task<int> GetTotalLessonsInCourseAsync(int courseId) =>
        await context.Lessons.CountAsync(l => l.Section.CourseId == courseId);

    public async Task<int> GetCompletedLessonsCountAsync(string studentId, int courseId) =>
        await context.StudentLessons.CountAsync(sl => 
            sl.StudentId == studentId && 
            sl.Lesson.Section.CourseId == courseId && 
            sl.IsCompleted);

    public async Task<StudentCourse?> GetStudentCourseAsync(string studentId, int courseId) =>
        await context.StudentCourses.FirstOrDefaultAsync(sc => sc.StudentId == studentId && sc.CourseId == courseId);
}