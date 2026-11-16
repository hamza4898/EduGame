using Ardalis.Result;
using EduGame.Application.Common.Contracts;
using MediatR;

namespace EduGame.Application.Friends.GetMyFriends;

public record GetMyFriendsQuery(string UserId) : IRequest<Result<List<FriendResponseDto>>>;