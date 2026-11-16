using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Auth.ChangePassword;

public record ChangePasswordCommand(Guid SessionId, string NewPassword) : IRequest<Result<LoginResponseDto>>;