using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Users.GetMySecurityPassport;

public record GetMySecurityPassportQuery(string MyId) : IRequest<Result<MySecurityPassportResponseDto>>;