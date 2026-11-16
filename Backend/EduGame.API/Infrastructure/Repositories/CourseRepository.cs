using EduGame.Infrastructure.Data;
using EduGame.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper.QueryableExtensions;
using EduGame.Application.Common.Interfaces;
using EduGame.Application.Courses.GetCourse;
using EduGame.Application.Courses.GetMyTeacherCourseCompletion;
using EduGame.Application.Courses.GetMyTeacherCourseProgress;
using EduGame.Application.Courses.GetMyTeacherCourseRatings;
using EduGame.Application.Courses.GetMyTeacherCourseStudentCounts;
using EduGame.Application.Common.Contracts;

namespace EduGame.Infrastructure.Repositories;

public class CourseRepository(EduGameDbContext context) : Repository<Course>(context), ICourseRepository
{
    public async Task<List<Course>> GetCoursesByTeacherId(string teacherId, CancellationToken cancellationToken) =>
        await Context.Courses
            .AsNoTracking()
            .Include(course => course.Community)
            .Include(course => course.SkillSubSection)
                .ThenInclude(section => section.SkillPath)
            .Include(course => course.Sections)
                .ThenInclude(section => section.Lessons)
                    .ThenInclude(lesson => lesson.Blocks)
            .Include(course => course.News)
            .Where(course => course.Community.TeacherId == teacherId
                || course.TeacherAccess.Any(access => access.TeacherId == teacherId))
            .OrderByDescending(course => course.Id)
            .ToListAsync(cancellationToken);

    public IQueryable<Course> BaseQuery() => 
        Context.Courses.AsNoTracking().Where(course => course.IsPublished);

    public IQueryable<Course> FilterBySearch(IQueryable<Course> query, string searchText) =>
        query.Where(c => c.Title.ToLower().Contains(searchText.ToLower()) 
                      || c.Description.ToLower().Contains(searchText.ToLower()));

    public IQueryable<Course> FilterByPrice(IQueryable<Course> query, string priceFilter) =>
        priceFilter.ToLower() == "free" ? query.Where(c => !c.IsPaid) : query.Where(c => c.IsPaid);

    public IQueryable<Course> FilterByPath(IQueryable<Course> query, int pathId) =>
        query.Where(c => c.SkillSubSection.SkillPathId == pathId);

    public IQueryable<Course> FilterBySubPath(IQueryable<Course> query, int subPathId) =>
        query.Where(c => c.SkillSubSectionId == subPathId);

    public async Task<List<CatalogCoursesDto>> ExecuteToCatalogListAsync(IQueryable<Course> query, AutoMapper.IConfigurationProvider configurationProvider) =>
        await query
            .ProjectTo<CatalogCoursesDto>(configurationProvider)
            .ToListAsync();

    public async Task<List<CatalogCoursesDto>> GetCoursesByIdsAsync(List<int> courseIds, AutoMapper.IConfigurationProvider configurationProvider) =>
        await Context.Courses
            .AsNoTracking()
            .Where(c => courseIds.Contains(c.Id))
            .ProjectTo<CatalogCoursesDto>(configurationProvider)
            .ToListAsync();

    public async Task<CourseDetailDto?> GetCourseDetailAsync(int id, string studentId, AutoMapper.IConfigurationProvider configurationProvider)
    {
        var course = await Context.Courses
            .AsNoTracking()
            .Include(c => c.Sections)
                .ThenInclude(section => section.Lessons)
                    .ThenInclude(lesson => lesson.Blocks)
            .Where(c => c.Id == id
                && (c.IsPublished || Context.StudentCourses.Any(enrollment =>
                    enrollment.CourseId == c.Id && enrollment.StudentId == studentId)))
            .FirstOrDefaultAsync();

        if (course is null)
            return null;

        return new CourseDetailDto
        {
            Sections = course.Sections.Select(section => new SectionNestedDto
            {
                Id = section.Id,
                Title = section.Title,
                Order = section.Order,
                Lessons = section.Lessons.Select(lesson => new LessonNestedDto
                {
                    Id = lesson.Id,
                    Title = lesson.Title,
                    Order = lesson.Order,
                    Blocks = lesson.Blocks.Select(block => new BlockNestedDto
                    {
                        Id = block.Id,
                        Type = (int)block.Type,
                        Content = block.Content,
                        Order = block.Order
                    }).ToList()
                }).ToList()
            }).ToList()
        };
    }

    public async Task<Course?> GetCourseInfoAsync(int courseId) =>
        await Context.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.Id == courseId);

    public async Task<Course?> GetCourseWithCommunityAsync(int courseId) =>
        await Context.Courses
            .AsNoTracking()
            .Include(c => c.Community)
            .FirstOrDefaultAsync(c => c.Id == courseId);

    public async Task<Course?> GetCourseForUpdateAsync(int courseId) =>
        await Context.Courses
            .Include(c => c.Community)
            .FirstOrDefaultAsync(c => c.Id == courseId);

    public async Task<Course?> GetCourseWithContentForUpdateAsync(int courseId) =>
        await Context.Courses
            .Include(c => c.Community)
            .Include(c => c.Sections)
                .ThenInclude(section => section.Lessons)
                    .ThenInclude(lesson => lesson.Blocks)
            .FirstOrDefaultAsync(c => c.Id == courseId);

    public async Task SaveChangesAsync(CancellationToken cancellationToken) =>
        await Context.SaveChangesAsync(cancellationToken);

    public async Task<GetCourseDescriptionResponseDto?> GetCourseDescription(int courseId) =>
        await Context.Courses
            .AsNoTracking()
            .Where(c => c.Id == courseId)
            .Select(c => new GetCourseDescriptionResponseDto(
                c.Title,
                c.Description,
                c.SkillSubSection.SkillPath.Name,
                c.SkillSubSection.Name,
                c.SkillSubSectionId,
                (int)c.Difficulty,
                c.Sections
                    .OrderBy(section => section.Order)
                    .Select(section => new CourseContentSectionDto(
                        section.Id,
                        section.Title,
                        section.Order,
                        section.Lessons
                            .OrderBy(lesson => lesson.Order)
                            .Select(lesson => new CourseContentLessonDto(
                                lesson.Id,
                                lesson.Title,
                                lesson.Order,
                                lesson.XpReward,
                                lesson.Blocks
                                    .OrderBy(block => block.Order)
                                    .Select(block => new CourseContentBlockDto(
                                        block.Id,
                                        block.Type,
                                        block.Content,
                                        block.Order))
                                    .ToList()))
                            .ToList()))
                    .ToList(),
                c.IsPaid,
                c.PriceCoins,
                c.IsPublished,
                c.CertificateEnabled,
                c.CertificateSigner))
            .FirstOrDefaultAsync();

    public async Task<CourseLike?> GetUserCourseLike(string userId, int courseId) =>
        await Context.CourseLikes
            .FirstOrDefaultAsync(cl => cl.UserId == userId && cl.CourseId == courseId);

    public void DeleteUserCourseLike(CourseLike courseLike) =>
        Context.CourseLikes.Remove(courseLike);

    public void AddUserCourseLike(CourseLike courseLike) =>
        Context.CourseLikes.Add(courseLike);

    public async Task<CourseArchive?> GetUserCourseArchive(string userId, int courseId) =>
        await Context.CourseArchives
            .FirstOrDefaultAsync(cl => cl.UserId == userId && cl.CourseId == courseId);

    public void DeleteUserCourseArchive(CourseArchive courseArchive) =>
        Context.CourseArchives.Remove(courseArchive);

    public void AddUserCourseArchive(CourseArchive courseArchive) =>
        Context.CourseArchives.Add(courseArchive);

    public async Task<bool> HasTeacherAccessAsync(int courseId, string teacherId, CancellationToken cancellationToken) =>
        await Context.CourseTeacherAccesses.AnyAsync(
            access => access.CourseId == courseId && access.TeacherId == teacherId,
            cancellationToken);

    public async Task<HashSet<int>> GetUserCourseLikesAsync(string userId, List<int> courseIds, CancellationToken cancellationToken) =>
        (await Context.CourseLikes
            .AsNoTracking()
            .Where(cl => cl.UserId == userId && courseIds.Contains(cl.CourseId))
            .Select(cl => cl.CourseId)
            .ToListAsync(cancellationToken))
            .ToHashSet();

public async Task<HashSet<int>> GetUserCourseArchivesAsync(string userId, List<int> courseIds, CancellationToken cancellationToken) =>
    (await Context.CourseArchives
        .AsNoTracking()
        .Where(ca => ca.UserId == userId && courseIds.Contains(ca.CourseId))
        .Select(ca => ca.CourseId)
        .ToListAsync(cancellationToken))
        .ToHashSet();

    public async Task<bool> IsCourseExistsAsync(int courseId, CancellationToken cancellationToken) =>
        await Context.Courses.AnyAsync(c => c.Id == courseId, cancellationToken); 

    public async Task<bool> CanEditCourseAsync(string userId, int courseId, CancellationToken cancellationToken)
    {
        var isOwner = await Context.Communities.AnyAsync(
            community => Context.Courses.Any(c => c.Id == courseId && c.CommunityId == community.Id) 
                         && community.TeacherId == userId, cancellationToken);

        var hasAccess = await Context.CourseTeacherAccesses.AnyAsync(
            access => access.CourseId == courseId && access.TeacherId == userId, cancellationToken);

        return isOwner || hasAccess;
    }

    public async Task<bool> IsCoursePublishedAsync(int courseId, CancellationToken cancellationToken) =>
        await Context.Courses.AnyAsync(c => c.Id == courseId && c.IsPublished, cancellationToken);
    
    public async Task<List<TeacherCourseCompletionCountDto>> GetTeacherCoursesCompletionCountsAsync(string teacherId, CancellationToken cancellationToken)
    {
        var courseIdsQuery = Context.Courses
            .Where(course => course.Community.TeacherId == teacherId
                || course.TeacherAccess.Any(access => access.TeacherId == teacherId))
            .Select(course => course.Id);

        return await Context.Set<StudentCourse>()
            .Where(enrollment => courseIdsQuery.Contains(enrollment.CourseId) && enrollment.IsCompleted)
            .GroupBy(enrollment => enrollment.CourseId)
            .Select(group => new TeacherCourseCompletionCountDto(group.Key, group.Count()))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TeacherCourseProgressDto>> GetTeacherCoursesProgressAsync(string teacherId, CancellationToken cancellationToken)
    {
        var courseIdsQuery = Context.Courses
            .Where(course => course.Community.TeacherId == teacherId
                || course.TeacherAccess.Any(access => access.TeacherId == teacherId))
            .Select(course => course.Id);

        return await Context.Set<StudentCourse>()
            .Where(enrollment => courseIdsQuery.Contains(enrollment.CourseId))
            .GroupBy(enrollment => enrollment.CourseId)
            .Select(group => new TeacherCourseProgressDto(
                group.Key,
                group.Average(enrollment => (double)enrollment.ProgressPercent)))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TeacherCourseRatingDto>> GetTeacherCoursesRatingsAsync(string teacherId, CancellationToken cancellationToken)
    {
        var courseIdsQuery = Context.Courses
            .Where(course => course.Community.TeacherId == teacherId
                || course.TeacherAccess.Any(access => access.TeacherId == teacherId))
            .Select(course => course.Id);

        return await Context.Set<CourseReview>()
            .Where(review => courseIdsQuery.Contains(review.CourseId))
            .GroupBy(review => review.CourseId)
            .Select(group => new TeacherCourseRatingDto(
                group.Key,
                group.Average(review => (double)review.Stars)))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TeacherCourseStudentCountDto>> GetTeacherCoursesStudentCountsAsync(string teacherId, CancellationToken cancellationToken)
    {
        var courseIdsQuery = Context.Courses
            .Where(course => course.Community.TeacherId == teacherId
                || course.TeacherAccess.Any(access => access.TeacherId == teacherId))
            .Select(course => course.Id);

        return await Context.Set<StudentCourse>()
            .Where(enrollment => courseIdsQuery.Contains(enrollment.CourseId))
            .GroupBy(enrollment => enrollment.CourseId)
            .Select(group => new TeacherCourseStudentCountDto(group.Key, group.Count()))
            .ToListAsync(cancellationToken);
    }

    public async Task<HashSet<int>> GetArchivedCourseIdsAsync(string userId, CancellationToken cancellationToken) =>
        await Context.Set<CourseArchive>()
            .Where(archive => archive.UserId == userId)
            .Select(archive => archive.CourseId)
            .ToHashSetAsync(cancellationToken);

    public async Task<HashSet<int>> GetLikedCourseIdsAsync(string userId, CancellationToken cancellationToken) =>
        await Context.Set<CourseLike>()
            .Where(like => like.UserId == userId)
            .Select(like => like.CourseId)
            .ToHashSetAsync(cancellationToken);

    public async Task<List<TeacherCourseStudentDto>> GetTeacherCourseStudentsAsync(string teacherId, CancellationToken cancellationToken)
    {
        var courseIds = await Context.Courses.AsNoTracking()
            .Where(course => course.Community.TeacherId == teacherId
                || course.TeacherAccess.Any(access => access.TeacherId == teacherId))
            .Select(course => course.Id)
            .ToArrayAsync(cancellationToken);

        if (courseIds.Length == 0)
            return new List<TeacherCourseStudentDto>();

        var enrollments = await Context.Set<StudentCourse>().AsNoTracking()
            .Where(enrollment => courseIds.Contains(enrollment.CourseId))
            .OrderBy(enrollment => enrollment.Course.Title)
            .ThenBy(enrollment => enrollment.StudentId)
            .Select(enrollment => new
            {
                enrollment.StudentId,
                StudentName = Context.Set<ApplicationUser>()
                    .Where(user => user.Id == enrollment.StudentId)
                    .Select(user => user.UserName)
                    .FirstOrDefault() ?? "Ученик",
                AvatarUrl = Context.Set<BaseUser>()
                    .Where(user => user.Id == enrollment.StudentId)
                    .Select(user => user.AvatarUrl)
                    .FirstOrDefault(),
                enrollment.CourseId,
                CourseTitle = enrollment.Course.Title,
                enrollment.ProgressPercent,
                enrollment.IsCompleted,
                enrollment.EnrolledAt
            })
            .ToListAsync(cancellationToken);

        return enrollments.Select(enrollment => new TeacherCourseStudentDto(
            enrollment.StudentId,
            enrollment.StudentName,
            enrollment.AvatarUrl,
            enrollment.CourseId,
            enrollment.CourseTitle,
            enrollment.ProgressPercent,
            enrollment.IsCompleted,
            new DateTimeOffset(DateTime.SpecifyKind(enrollment.EnrolledAt, DateTimeKind.Utc))))
            .ToList();
    }
}