using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Friends.SearchUsers;

public class SearchUsersQueryHandler(IFriendshipRepository friendshipRepository) 
    : IRequestHandler<SearchUsersQuery, Result<List<UserSearchResponseDto>>>
{
    public async Task<Result<List<UserSearchResponseDto>>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
    {
        var normalizedQuery = request.Query?.Trim() ?? string.Empty;

        if (normalizedQuery.Length < 2) return Result.Success(new List<UserSearchResponseDto>());

        var clampedLimit = Math.Clamp(request.Limit, 1, 25);

        var users = await friendshipRepository.SearchUsers(request.UserId, normalizedQuery, clampedLimit);

        return Result.Success(users);
    }
}