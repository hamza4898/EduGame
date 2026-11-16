using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Auth.Login;

public record LoginUserCommand(string Email, string Password) : IRequest<Result<LoginResponseDto>>;