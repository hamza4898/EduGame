using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Developer.GetDeveloperUpdates;

public record GetDeveloperUpdatesQuery()
    : IRequest<Result<IReadOnlyList<DeveloperUpdateResponseDto>>>;