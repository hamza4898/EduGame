using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Developer.GetDeveloperAdminAccess;

public record GetDeveloperAdminAccessQuery(string UserId) : IRequest<Result<bool>>;