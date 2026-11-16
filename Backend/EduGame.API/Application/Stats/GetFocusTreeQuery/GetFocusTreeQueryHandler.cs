using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Application.Stats.GetFocusTree;

public class GetFocusTreeQueryHandler(ISkillTreeRepository repository)
    : IRequestHandler<GetFocusTreeQuery, Result<List<FocusNodeDto>>>
{
    public async Task<Result<List<FocusNodeDto>>> Handle(GetFocusTreeQuery request, CancellationToken cancellationToken)
    {
        var cleanPathCode = request.PathCode.ToLower().Trim();

        var pathId = await repository.GetPathIdByCodeAsync(cleanPathCode);
        if (pathId == 0) return Result.NotFound("Путь обучения не найден в СУБД.");

        var userXpMap = await repository.GetUserXpMapByPathIdAsync(request.UserId, pathId);

        var nodes = await repository.GetFocusNodesByPathIdAsync(pathId);

        foreach (var node in nodes)
        {
            userXpMap.TryGetValue(node.SkillSubSectionId, out var xp);
            node.CurrentUserXp = xp; 
        }

        return Result.Success(nodes);
    }
}