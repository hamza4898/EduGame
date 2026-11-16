using Ardalis.Result;
using MediatR;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Courses.GetMyNotifications;

public sealed record GetMyNotificationsQuery(string UserId) 
    : IRequest<Result<List<NotificationResponseDto>>>;