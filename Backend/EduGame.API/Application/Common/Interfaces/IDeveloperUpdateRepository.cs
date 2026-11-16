using EduGame.Domain.Entities;

namespace EduGame.Application.Common.Interfaces;

public interface IDeveloperUpdateRepository
{
    Task<IReadOnlyList<DeveloperUpdate>> GetLatestAsync(int limit, CancellationToken cancellationToken);
}