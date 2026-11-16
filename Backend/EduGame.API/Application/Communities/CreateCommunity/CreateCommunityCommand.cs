using Ardalis.Result;
using MediatR;

namespace EduGame.Application.Communities.CreateCommunity;

public record CreateCommunityCommand(CreateCommunityDto Dto, string UserId, string UserName) : IRequest<Result<Guid>>;