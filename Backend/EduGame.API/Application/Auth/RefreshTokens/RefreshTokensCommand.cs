using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Auth.RefreshTokens;

public record RefreshTokensCommand(string? RefreshToken) : IRequest<Result<TokenResponseDto>>;