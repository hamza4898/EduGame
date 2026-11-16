using EduGame.Infrastructure.Data;
using EduGame.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using EduGame.Application.Common.Interfaces;
using EduGame.Application.Common.Contracts;

namespace EduGame.Infrastructure.Repositories;

public class UserRepository(EduGameDbContext context) : Repository<BaseUser>(context), IUserRepository
{
    public async Task<Student?> GetStudentById(string externalId) =>
        await Context.Students
            .FirstOrDefaultAsync(user => EF.Property<string>(user, "Id") == externalId);

    public async Task<Teacher?> GetTeacherById(string externalId) =>
        await Context.Teachers
            .AsNoTracking()
            .FirstOrDefaultAsync(user => EF.Property<string>(user, "Id") == externalId);

    public async Task<Student?> GetStudentWithNicknameAsync(string studentId) =>
        await Context.Students
            .AsNoTracking()
            .Where(s => s.Id == studentId)
            .Select(s => new Student
            {
                Id = s.Id,
                CoinsBalance = s.CoinsBalance,
                GlobalXp = s.GlobalXp,
                AvatarUrl = s.AvatarUrl,
            })
            .FirstOrDefaultAsync();

    public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken) =>
        await Context.Users.AnyAsync(user => user.Id == id, cancellationToken);

    public async Task<bool> IsInRoleAsync(string userId, string roleName, CancellationToken cancellationToken) =>
        await Context.UserRoles
            .Join(Context.Roles, 
                userRole => userRole.RoleId, 
                role => role.Id, 
                (userRole, role) => new { userRole, role })
            .AnyAsync(x => x.userRole.UserId == userId && x.role.Name == roleName, cancellationToken);

    public async Task<string?> GetUserNameAsync(string id, CancellationToken cancellationToken) =>
        await Context.Users
            .Where(user => user.Id == id)
            .Select(user => user.UserName)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<List<TeacherXpAwardDto>> GetTopTeacherXpAwardsAsync(string teacherId, int count, CancellationToken cancellationToken) =>
        await (
            from award in Context.Set<TeacherXpAward>().AsNoTracking()
            join course in Context.Courses.AsNoTracking() on award.CourseId equals course.Id
            where award.TeacherId == teacherId
            orderby award.CreatedAt descending
            select new TeacherXpAwardDto(
                award.Reason,
                award.CourseId,
                course.Title,
                course.Difficulty,
                award.Xp,
                new DateTimeOffset(DateTime.SpecifyKind(award.CreatedAt, DateTimeKind.Utc))))
            .Take(count)
            .ToListAsync(cancellationToken);
            
    public async Task<List<RawTeacherDataDto>> GetRawTeachersAnalyticsAsync(CancellationToken cancellationToken)
    {
        return await Context.Teachers.AsNoTracking()
            .Select(t => new RawTeacherDataDto
            {
                Id = t.Id,
                UserName = t.ApplicationUser.UserName ?? "Преподаватель",
                AvatarUrl = t.AvatarUrl,
                Courses = Context.Courses.AsNoTracking()
                    .Where(c => c.IsPublished && c.Community.TeacherId == t.Id)
                    .Select(c => new RawCourseMetricsDto
                    {
                        AverageProgress = Context.StudentCourses.Where(e => e.CourseId == c.Id).Select(e => (double?)e.ProgressPercent).Average() ?? 0,
                        AverageRating = c.Reviews.Select(r => (double?)r.Stars).Average() ?? 0
                    }).ToList(),
                UniqueStudentsCount = Context.StudentCourses.AsNoTracking()
                    .Where(e => e.Course.IsPublished && e.Course.Community.TeacherId == t.Id)
                    .Select(e => e.StudentId)
                    .Distinct()
                    .Count()
            })
            .Where(t => t.Courses.Count > 0)
            .ToListAsync(cancellationToken);
    }
}