using EduGame.Application.Common.Contracts;
using EduGame.Domain.Entities;

namespace EduGame.Application.Common.Interfaces;

public interface IUserRepository : IRepository<BaseUser>
{
    Task<Student?> GetStudentById(string externalId);

    Task<Teacher?> GetTeacherById(string externalId);
    
    Task<Student?> GetStudentWithNicknameAsync(string studentId);

    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken);

    Task<bool> IsInRoleAsync(string userId, string roleName, CancellationToken cancellationToken);

    Task<string?> GetUserNameAsync(string id, CancellationToken cancellationToken);

    Task<List<TeacherXpAwardDto>> GetTopTeacherXpAwardsAsync(string teacherId, int count, CancellationToken cancellationToken);

    Task<List<RawTeacherDataDto>> GetRawTeachersAnalyticsAsync(CancellationToken cancellationToken);
}