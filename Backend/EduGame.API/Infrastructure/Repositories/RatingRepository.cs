using AutoMapper.QueryableExtensions;
using EduGame.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using EduGame.Application.Communities.GetCommunityRatings;
using EduGame.Application.Courses.GetCourseRatings;
using EduGame.Application.Common.Interfaces;
using EduGame.Application.Courses.GetStudentsRating;

namespace EduGame.Infrastructure.Repositories;

public class RatingRepository(EduGameDbContext context) : IRatingRepository
{
    public async Task<List<CourseRawRatingDto>> GetRawCourseRatingsAsync(AutoMapper.IConfigurationProvider mapperConfig, CancellationToken cancellationToken) =>
        await context.Courses
            .AsNoTracking()
            .Where(course => course.IsPublished)
            .Select(course => new CourseRawRatingDto
            {
                Id = course.Id,
                Title = course.Title,
                Rating = course.Reviews.Select(review => (double?)review.Stars).Average() ?? 0,
                StudentsCount = context.StudentCourses.Count(enrollment => enrollment.CourseId == course.Id),
                AvgProgress = context.StudentCourses
                    .Where(enrollment => enrollment.CourseId == course.Id)
                    .Select(enrollment => (double?)enrollment.ProgressPercent)
                    .Average() ?? 0
            })
            .ToListAsync(cancellationToken);

    public async Task<List<CommunityRawRatingDto>> GetRawCommunityRatingsAsync(AutoMapper.IConfigurationProvider mapperConfig, CancellationToken cancellationToken) =>
        await context.Communities
            .AsNoTracking()
            .Select(community => new CommunityRawRatingDto
            {
                Id = community.Id,
                Name = community.CommunityName,
                TotalXp = context.UserSubSectionXps
                    .Where(xp => context.CommunityMembers.Any(member =>
                        member.CommunityId == community.CommunityId
                        && member.UserId == xp.UserId
                        && community.Courses.Select(course => course.SkillSubSectionId)
                            .Contains(xp.SkillSubSectionId)))
                    .Sum(xp => (int?)xp.Xp) ?? 0,
                MaxPossibleXp = (community.Courses
                    .SelectMany(course => course.Sections)
                    .SelectMany(section => section.Lessons)
                    .Sum(lesson => (int?)lesson.XpReward) ?? 0)
                    * context.CommunityMembers.Count(member => member.CommunityId == community.CommunityId)
            })
            .ToListAsync(cancellationToken);

    public async Task<List<StudentRatingResponseDto>> GetTopStudentsRawAsync(AutoMapper.IConfigurationProvider mapperConfig, CancellationToken cancellationToken) =>
        await context.Students
            .AsNoTracking()
            .OrderByDescending(s => s.GlobalXp)
            .Take(10)
            .ProjectTo<StudentRatingResponseDto>(mapperConfig)
            .ToListAsync(cancellationToken);
}