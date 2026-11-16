using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Users.GetMyBusinessPassport;

public record GetMyBusinessPassportQuery(string UserId, string UserRole) : IRequest<Result<MyBusinessPassportResponseDto>>;