using EduGame.Domain.Entities;
using EduGame.Application.Courses.GetMyTeacherCourseCompletion;
using EduGame.Application.Courses.GetMyTeacherCourseProgress;
using EduGame.Application.Courses.GetMyTeacherCourseRatings;
using EduGame.Application.Courses.GetMyTeacherCourseStudentCounts;
using EduGame.Application.Common.Contracts;
using EduGame.Application.Courses.GetCourse;

namespace EduGame.Application.Common.Interfaces;

public interface ICourseRepository : IRepository<Course>
{
    Task<List<Course>> GetCoursesByTeacherId(string teacherId, CancellationToken cancellationToken);

    IQueryable<Course> BaseQuery();
    
    IQueryable<Course> FilterBySearch(IQueryable<Course> query, string searchText);

    IQueryable<Course> FilterByPrice(IQueryable<Course> query, string priceFilter);

    IQueryable<Course> FilterByPath(IQueryable<Course> query, int pathId);

    IQueryable<Course> FilterBySubPath(IQueryable<Course> query, int subPathId);

    Task<List<CatalogCoursesDto>> ExecuteToCatalogListAsync(IQueryable<Course> query, AutoMapper.IConfigurationProvider configurationProvider);

    Task<List<CatalogCoursesDto>> GetCoursesByIdsAsync(List<int> courseIds, AutoMapper.IConfigurationProvider configurationProvider);

    Task<CourseDetailDto?> GetCourseDetailAsync(int id, string studentId, AutoMapper.IConfigurationProvider configurationProvider);

    Task<Course?> GetCourseInfoAsync(int courseId);

    Task<Course?> GetCourseWithCommunityAsync(int courseId);

    Task<Course?> GetCourseForUpdateAsync(int courseId);

    Task<Course?> GetCourseWithContentForUpdateAsync(int courseId);

    Task<GetCourseDescriptionResponseDto?> GetCourseDescription(int courseId);

    Task SaveChangesAsync(CancellationToken cancellationToken);

    Task<CourseLike?> GetUserCourseLike(string userId, int courseId);

    void DeleteUserCourseLike(CourseLike courseLike);

    void AddUserCourseLike(CourseLike courseLike);

    Task<CourseArchive?> GetUserCourseArchive(string userId, int courseId);

    void DeleteUserCourseArchive(CourseArchive courseArchive);

    void AddUserCourseArchive(CourseArchive courseArchive);

    Task<bool> HasTeacherAccessAsync(int courseId, string teacherId, CancellationToken cancellationToken);

    Task<HashSet<int>> GetUserCourseLikesAsync(string userId, List<int> courseIds, CancellationToken cancellationToken);

    Task<HashSet<int>> GetUserCourseArchivesAsync(string userId, List<int> courseIds, CancellationToken cancellationToken);

    Task<bool> IsCourseExistsAsync(int courseId, CancellationToken cancellationToken);

    Task<bool> CanEditCourseAsync(string userId, int courseId, CancellationToken cancellationToken);

    Task<bool> IsCoursePublishedAsync(int courseId, CancellationToken cancellationToken);

    Task<List<TeacherCourseCompletionCountDto>> GetTeacherCoursesCompletionCountsAsync(string teacherId, CancellationToken cancellationToken);

    Task<List<TeacherCourseProgressDto>> GetTeacherCoursesProgressAsync(string teacherId, CancellationToken cancellationToken);

    Task<List<TeacherCourseRatingDto>> GetTeacherCoursesRatingsAsync(string teacherId, CancellationToken cancellationToken);

    Task<List<TeacherCourseStudentDto>> GetTeacherCourseStudentsAsync(string teacherId, CancellationToken cancellationToken);

    Task<List<TeacherCourseStudentCountDto>> GetTeacherCoursesStudentCountsAsync(string teacherId, CancellationToken cancellationToken);

    Task<HashSet<int>> GetArchivedCourseIdsAsync(string userId, CancellationToken cancellationToken);

    Task<HashSet<int>> GetLikedCourseIdsAsync(string userId, CancellationToken cancellationToken);
}