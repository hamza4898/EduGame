using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Application.Stats.GetSkillPathZones;

public class GetSkillPathZonesQueryHandler(
    ISkillTreeRepository repository,
    IRatingCalculatorService ratingCalculatorService
) : IRequestHandler<GetSkillPathZonesQuery, Result<List<SkillPathZoneDto>>>
{
    public async Task<Result<List<SkillPathZoneDto>>> Handle(GetSkillPathZonesQuery request, CancellationToken cancellationToken)
    {
        var skillPaths = await repository.GetSkillPathsWithSubSectionsAsync();

        var userXpRecords = await repository.GetUserSubSectionXpAsync(request.UserId);

        var pathZones = ratingCalculatorService.CalculateSkillPathZones(skillPaths, userXpRecords);

        return Result.Success(pathZones);
    }
}