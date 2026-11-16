using MediatR;
using Ardalis.Result;
using EduGame.Domain.Entities;

namespace EduGame.Application.Friends.GetFriendsAvailableForCommunity;

public record GetFriendsAvailableForCommunityQuery(
    string UserId, 
    Guid CommunityId
) : IRequest<Result<List<BaseUser>>>;