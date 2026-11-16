using EduGame.Infrastructure.Data;
using EduGame.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using EduGame.Application.Common.Interfaces;
using EduGame.Application.Common.Contracts;

namespace EduGame.Infrastructure.Repositories;

public class ChatRepository(EduGameDbContext context) : Repository<AiSupportMessage>(context), IChatRepository
{
    public async Task<List<AiSupportMessageResponseDto>> GetAiSupportHistoryAsync(string userId, int limit, CancellationToken cancellationToken) =>
        await Context.Set<AiSupportMessage>()
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.CreatedAt)
            .ThenByDescending(m => m.Id)
            .Take(limit)
            .OrderBy(m => m.CreatedAt)
            .ThenBy(m => m.Id)
            .Select(m => new AiSupportMessageResponseDto(m.Id, m.Role, m.Content, m.CreatedAt))
            .ToListAsync(cancellationToken);

    public async Task SaveAiSupportExchangeAsync(AiSupportMessage userMessage, AiSupportMessage assistantMessage, CancellationToken cancellationToken) =>
        await Context.Set<AiSupportMessage>().AddRangeAsync([userMessage, assistantMessage], cancellationToken);
}