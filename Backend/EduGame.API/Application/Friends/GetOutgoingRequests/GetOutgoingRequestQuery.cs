using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Friends.GetOutgoingRequests;

public record GetOutgoingRequestQuery(string UserId) : IRequest<Result<List<FriendshipRequestResponseDto>>>;