using EduGame.Infrastructure.Data;
using EduGame.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using EduGame.Application.Common.Interfaces;
using EduGame.Domain.Enums;

namespace EduGame.Infrastructure.Services;

public sealed class TeacherProgressionService(EduGameDbContext context) : ITeacherProgressionService
{
    private const int CoursePublishedXp = 100;
    private const int StudentCompletionXp = 75;
    private const int CourseReviewXp = 25;
    private const int MaxRewardedCompletionsPerCourse = 50;
    private const int MaxRewardedReviewsPerCourse = 50;
    private static readonly TimeSpan MinimumStudentAccountAge = TimeSpan.FromDays(7);
    private static readonly (int Students, int Xp)[] CompletionMilestones =
    [
        (5, 100),
        (10, 250),
        (25, 500)
    ];

    public async Task AwardCoursePublishedAsync(int courseId, CancellationToken cancellationToken)
    {
        var course = await context.Courses.AsNoTracking()
            .Where(item => item.Id == courseId && item.IsPublished)
            .Select(item => new { item.Id, TeacherId = item.Community.TeacherId, item.Difficulty })
            .SingleOrDefaultAsync(cancellationToken);
        if (course?.TeacherId is null)
            return;

        await AwardAsync(course.TeacherId, courseId, null, $"course:{courseId}:published",
            "PublishedCourse", ScaleXp(CoursePublishedXp, course.Difficulty), cancellationToken);
    }

    public async Task AwardCourseCompletionAsync(int courseId, string studentId, CancellationToken cancellationToken)
    {
        var minimumRegisteredAt = DateTime.UtcNow.Subtract(MinimumStudentAccountAge);
        var course = await context.Courses.AsNoTracking()
            .Where(item => item.Id == courseId && item.IsPublished)
            .Select(item => new
            {
                item.Id,
                TeacherId = item.Community.TeacherId,
                item.Difficulty,
                IsEnrolledAndCompleted = context.StudentCourses.Any(enrollment =>
                    enrollment.CourseId == item.Id
                    && enrollment.StudentId == studentId
                    && enrollment.IsCompleted
                    && enrollment.ProgressPercent >= 100),
                IsVerifiedStudent = context.Students.Any(student => student.Id == studentId)
                    && context.Users.Any(user => user.Id == studentId
                        && user.EmailConfirmed
                        && user.RegisteredAt <= minimumRegisteredAt)
                    && !context.Teachers.Any(teacher => teacher.Id == studentId)
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (course?.TeacherId is null
            || course.TeacherId == studentId
            || !course.IsEnrolledAndCompleted
            || !course.IsVerifiedStudent)
            return;

        var completionCount = await GetEligibleCompletionCountAsync(courseId, cancellationToken);
        await AwardAsync(course.TeacherId, courseId, studentId, $"course:{courseId}:completed:{studentId}",
            "StudentCompletion", ScaleXp(StudentCompletionXp, course.Difficulty), cancellationToken, MaxRewardedCompletionsPerCourse);

        foreach (var milestone in CompletionMilestones.Where(item => completionCount >= item.Students))
        {
            await AwardAsync(course.TeacherId, courseId, null,
                $"course:{courseId}:completion-milestone:{milestone.Students}",
                "CompletionMilestone", ScaleXp(milestone.Xp, course.Difficulty), cancellationToken);
        }

        await AwardCourseReviewAsync(courseId, studentId, cancellationToken);
    }

    public async Task AwardCourseReviewAsync(int courseId, string studentId, CancellationToken cancellationToken)
    {
        var minimumRegisteredAt = DateTime.UtcNow.Subtract(MinimumStudentAccountAge);
        var review = await context.CourseReviews.AsNoTracking()
            .Where(item => item.CourseId == courseId && item.StudentId == studentId && item.Text.Length >= 30)
            .Select(item => new
            {
                item.CourseId,
                item.StudentId,
                TeacherId = item.Course.Community.TeacherId,
                item.Course.Difficulty,
                IsVerifiedStudent = context.Students.Any(student => student.Id == item.StudentId)
                    && context.Users.Any(user => user.Id == item.StudentId
                        && user.EmailConfirmed
                        && user.RegisteredAt <= minimumRegisteredAt)
                    && !context.Teachers.Any(teacher => teacher.Id == item.StudentId),
                IsCourseCompleted = context.StudentCourses.Any(enrollment =>
                    enrollment.CourseId == item.CourseId
                    && enrollment.StudentId == item.StudentId
                    && enrollment.IsCompleted
                    && enrollment.ProgressPercent >= 100)
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (review?.TeacherId is null
            || review.TeacherId == studentId
            || !review.IsVerifiedStudent
            || !review.IsCourseCompleted)
            return;

        await AwardAsync(review.TeacherId, courseId, studentId, $"course:{courseId}:review:{studentId}",
            "CourseReview", ScaleXp(CourseReviewXp, review.Difficulty), cancellationToken, MaxRewardedReviewsPerCourse);
    }

    private static int ScaleXp(int baseXp, CourseDifficulty difficulty)
    {
        var multiplier = difficulty switch
        {
            CourseDifficulty.Easy => 1,
            CourseDifficulty.Medium => 2,
            CourseDifficulty.Hard => 3,
            _ => throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, "Неизвестная сложность курса.")
        };
        return checked(baseXp * multiplier);
    }

    private Task<int> GetEligibleCompletionCountAsync(int courseId, CancellationToken cancellationToken)
    {
        var minimumRegisteredAt = DateTime.UtcNow.Subtract(MinimumStudentAccountAge);
        return
        context.StudentCourses.AsNoTracking()
            .Where(enrollment => enrollment.CourseId == courseId
                && enrollment.IsCompleted
                && enrollment.ProgressPercent >= 100
                && context.Students.Any(student => student.Id == enrollment.StudentId)
                && context.Users.Any(user => user.Id == enrollment.StudentId
                    && user.EmailConfirmed
                    && user.RegisteredAt <= minimumRegisteredAt)
                && !context.Teachers.Any(teacher => teacher.Id == enrollment.StudentId))
            .Select(enrollment => enrollment.StudentId)
            .Distinct()
            .CountAsync(cancellationToken);
    }

    private async Task AwardAsync(
        string teacherId,
        int courseId,
        string? studentId,
        string awardKey,
        string reason,
        int xp,
        CancellationToken cancellationToken,
        int? maxAwardsPerCourse = null)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var lockedTeacherId = await context.Database.SqlQuery<string>(
                $"""SELECT "Id" AS "Value" FROM "Teachers" WHERE "Id" = {teacherId} FOR UPDATE""")
            .SingleOrDefaultAsync(cancellationToken);
        if (lockedTeacherId is null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return;
        }

        if (await context.TeacherXpAwards.AnyAsync(award => award.AwardKey == awardKey, cancellationToken))
        {
            await transaction.CommitAsync(cancellationToken);
            return;
        }

        if (maxAwardsPerCourse is not null
            && await context.TeacherXpAwards.CountAsync(
                award => award.CourseId == courseId && award.Reason == reason,
                cancellationToken) >= maxAwardsPerCourse.Value)
        {
            await transaction.CommitAsync(cancellationToken);
            return;
        }

        var teacher = await context.Teachers.SingleAsync(item => item.Id == teacherId, cancellationToken);
        teacher.GlobalXp = checked(teacher.GlobalXp + xp);
        context.TeacherXpAwards.Add(new TeacherXpAward
        {
            TeacherId = teacherId,
            CourseId = courseId,
            StudentId = studentId,
            AwardKey = awardKey,
            Reason = reason,
            Xp = xp
        });
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
