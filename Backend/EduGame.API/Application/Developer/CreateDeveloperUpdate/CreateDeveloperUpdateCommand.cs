using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Developer.CreateDeveloperUpdate;

public sealed record CreateDeveloperUpdateCommand(CreateDeveloperUpdateRequest Request)
    : IRequest<Result<DeveloperUpdateResponseDto>>;