using EduGame.Application.Common.Contracts;
using EduGame.Domain.Entities;

namespace EduGame.Application.Common.Interfaces;

public interface IChatRepository : IRepository<AiSupportMessage>
{
    Task<List<AiSupportMessageResponseDto>> GetAiSupportHistoryAsync(string userId, int limit, CancellationToken cancellationToken);
    
    Task SaveAiSupportExchangeAsync(AiSupportMessage userMessage, AiSupportMessage assistantMessage, CancellationToken cancellationToken);
}