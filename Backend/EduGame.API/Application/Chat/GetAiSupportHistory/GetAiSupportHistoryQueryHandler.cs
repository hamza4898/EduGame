using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using EduGame.Application.Common.Contracts;
using MediatR;

namespace EduGame.Application.Chat.GetAiSupportHistory;

public sealed class GetAiSupportHistoryQueryHandler(IChatRepository chatRepository) : IRequestHandler<GetAiSupportHistoryQuery, Result<List<AiSupportMessageResponseDto>>>
{
    private const int HistoryMessageLimit = 100;

    public async Task<Result<List<AiSupportMessageResponseDto>>> Handle(GetAiSupportHistoryQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
            return Result.Unauthorized("Не удалось определить пользователя.");

        var messages = await chatRepository.GetAiSupportHistoryAsync(request.UserId, HistoryMessageLimit, cancellationToken);

        return Result.Success(messages);
    }
}