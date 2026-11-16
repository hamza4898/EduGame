using EduGame.Infrastructure.Data;
using EduGame.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Infrastructure.Repositories;

public sealed class DeveloperUpdateRepository(EduGameDbContext context) : IDeveloperUpdateRepository
{
    public async Task<IReadOnlyList<DeveloperUpdate>> GetLatestAsync(
        int limit,
        CancellationToken cancellationToken) =>
        await context.Set<DeveloperUpdate>()
            .AsNoTracking()
            .OrderByDescending(update => update.CreatedAt)
            .ThenByDescending(update => update.Id)
            .Take(limit)
            .ToListAsync(cancellationToken);
}
