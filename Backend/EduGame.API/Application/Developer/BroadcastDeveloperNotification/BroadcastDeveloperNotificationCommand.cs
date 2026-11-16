using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Developer.BroadcastDeveloperNotification;

public sealed record BroadcastDeveloperNotificationCommand(
    string SenderId,
    string Message) : IRequest<Result<int>>;