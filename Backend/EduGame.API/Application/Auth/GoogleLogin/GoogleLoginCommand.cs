using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Auth.GoogleLogin;

public record GoogleLoginCommand(string IdToken) : IRequest<Result<GoogleLoginResponseDto>>;