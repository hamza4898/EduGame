using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Auth.CompleteProfile;

public record CompleteProfileCommand(string UserId, CompleteProfileDto Dto) : IRequest<Result>;