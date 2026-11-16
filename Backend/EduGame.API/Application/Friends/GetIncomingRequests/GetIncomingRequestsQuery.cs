using Ardalis.Result;
using EduGame.Application.Common.Contracts;
using MediatR;

namespace EduGame.Application.Friends.GetIncomingRequests;

public record GetIncomingRequestsQuery(string UserId) : IRequest<Result<List<FriendshipRequestResponseDto>>>;