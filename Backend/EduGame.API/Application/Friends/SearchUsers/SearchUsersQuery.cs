using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Friends.SearchUsers;

public record SearchUsersQuery(string UserId, string Query, int Limit) 
    : IRequest<Result<List<UserSearchResponseDto>>>;