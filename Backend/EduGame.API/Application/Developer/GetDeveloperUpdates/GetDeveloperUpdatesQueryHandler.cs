using Ardalis.Result;
using MediatR;
using EduGame.Application.Common.Interfaces;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Developer.GetDeveloperUpdates;

public class GetDeveloperUpdatesQueryHandler(IDeveloperUpdateRepository repository)
    : IRequestHandler<GetDeveloperUpdatesQuery, Result<IReadOnlyList<DeveloperUpdateResponseDto>>>
{
    public async Task<Result<IReadOnlyList<DeveloperUpdateResponseDto>>> Handle(
        GetDeveloperUpdatesQuery request,
        CancellationToken cancellationToken)
    {
        var updates = await repository.GetLatestAsync(100, cancellationToken);
        return Result.Success<IReadOnlyList<DeveloperUpdateResponseDto>>(updates
            .Select(update => new DeveloperUpdateResponseDto(
                update.Id,
                update.Title,
                update.Category,
                update.Content,
                update.AuthorSignature,
                update.CreatedAt))
            .ToList());
    }
}