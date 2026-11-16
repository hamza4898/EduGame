using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Chat.ConfirmDelivery;

public record ConfirmDeliveryCommand(string MessageId, string GroupName) : IRequest<Result<bool>>;