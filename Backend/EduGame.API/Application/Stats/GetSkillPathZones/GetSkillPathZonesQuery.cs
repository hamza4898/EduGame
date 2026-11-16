using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Stats.GetSkillPathZones;

public record GetSkillPathZonesQuery(string UserId) : IRequest<Result<List<SkillPathZoneDto>>>;